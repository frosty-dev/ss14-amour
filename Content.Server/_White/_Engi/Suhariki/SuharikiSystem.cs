using Content.Server.Inventory;
using Content.Server.Nutrition.Components;
using Content.Server.Popups;
using Content.Shared.Interaction.Events;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Random;
using Content.Shared.Damage;
using Content.Server.Effects;
using Robust.Shared.Player;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Server.Chat.Systems;
using Content.Server.Nutrition.EntitySystems;
using Robust.Shared.Timing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Content.Shared.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Body.Components;
using Content.Server.Speech.Components;

namespace Content.Server._White._Engi.Suhariki;

/// <summary>
/// WD Engi Exclusive.
/// </summary>
public sealed class SuharikiSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly ColorFlashEffectSystem _color = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly FoodSystem _food = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BodySystem _body = default!;
    private EntityQuery<MetaDataComponent> _metaQuery;

    public override void Initialize()
    {
        base.Initialize();
        _metaQuery = GetEntityQuery<MetaDataComponent>();
        SubscribeLocalEvent<SuharikiComponent, UseInHandEvent>(OnUseInHand, after: new[] { typeof(OpenableSystem), typeof(ServerInventorySystem) });
        SubscribeLocalEvent<SuharikiComponent, GetVerbsEvent<AlternativeVerb>>(AddEatVerb);
    }

    private void OnUseInHand(Entity<SuharikiComponent> entity, ref UseInHandEvent ev)
    {
        if (ev.Handled)
            return;

        var result = TryUse(ev.User, ev.User, entity, entity.Comp);
        ev.Handled = result.Handled;
    }

    private (bool Success, bool Handled) TryUse(EntityUid user, EntityUid target, EntityUid food, SuharikiComponent scomponent)
    {
        var foodComponents = _metaQuery.GetComponent(food).EntityPrototype;

        if (foodComponents == null)
            return (false, false);

        foodComponents.Components.TryGetComponent("Food", out var component);

        if (component == null)
            return (false, false);

        // idk what I'm doing but otherwise the owner is "0" and it throws fatal error
        component.Owner = food;

        var result = _food.TryFeed(user, target, food, (FoodComponent) component);

        if (result.Success)
            Timer.Spawn(300, () => RollEvent(scomponent, user));

        return (true, true);
    }

    private void RollEvent(SuharikiComponent scomponent, EntityUid user)
    {
        if (scomponent.StonesInFood < 1)
            return;

        scomponent.StonesInFood -= 1;

        // Pseudorandom shit
        if (!_random.Prob(scomponent.Chance))
            return;

        // Normally slimes don't care about hard food
        var whomst = _metaQuery.GetComponent(user).EntityPrototype;
        var isUserSlime = whomst!.ID.Contains("slime", StringComparison.OrdinalIgnoreCase);
        if (isUserSlime)
            return;

        var modifiedDamage = _damageableSystem.TryChangeDamage(user, scomponent.Damage, true);
        var deleted = Deleted(user);
        if (modifiedDamage is null || !EntityManager.EntityExists(user))
            return;
        if (!modifiedDamage.Any() || deleted)
            return;
        // Damaging the user with appropriate effects
        var stringTopopup = Loc.GetString("suhariki-lost", ("user", user));
        _popup.PopupEntity(Name(user) + " " + stringTopopup, user, Shared.Popups.PopupType.LargeCaution);
        _color.RaiseEffect(Color.Red, new List<EntityUid> { user }, Filter.Pvs(user, entityManager: EntityManager));
        _audio.PlayPvs(scomponent.UseSound, user, AudioParams.Default.WithVolume(-1f));
        _adminLogger.Add(LogType.Damaged,
            LogImpact.Medium,
            $"This idiot {ToPrettyString(user):user} tried to eat Suhariki and lost tooth, received {modifiedDamage.GetTotal():damage} damage and recieved FrontalLisp accent");

        // Spawns prototype
        var coords = Transform(user).Coordinates;
        Spawn(scomponent.HoldingPrototype, coords.Offset(_random.NextVector2(0.2f)));

        // Scream
        Timer.Spawn(300, () => _chat.TryEmoteWithChat(user, scomponent.EmoteId));

        // Adding FrontalLisp accent
        EnsureComp<FrontalLispComponent>(user);

        return;

    }

    private void AddEatVerb(Entity<SuharikiComponent> entity, ref GetVerbsEvent<AlternativeVerb> ev)
    {
        if (entity.Owner == ev.User ||
            !ev.CanInteract ||
            !ev.CanAccess ||
            !TryComp<BodyComponent>(ev.User, out var body) ||
            !_body.TryGetBodyOrganComponents<StomachComponent>(ev.User, out var stomachs))
            return;

        var user = ev.User;
        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                TryUse(user, user, entity, entity.Comp);
            },
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/cutlery.svg.192dpi.png")),
            Text = Loc.GetString("food-system-verb-eat"),
            Priority = -1
        };

        ev.Verbs.Add(verb);
    }

}
