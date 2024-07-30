using System.Linq;
using Content.Server._White.Cult.Items.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._White.Chaplain;
using Content.Shared._White.Cult;
using Content.Shared._White.Cult.Components;
using Content.Shared._White.Cult.UI;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Cult.Items.Systems;

public sealed class MagicHandSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly HolyWeaponSystem _holyWeapon = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultStunHandComponent, MeleeHitEvent>(OnStunHit);
        SubscribeLocalEvent<CultRitesHandComponent, MeleeHitEvent>(OnRitesHit);
        SubscribeLocalEvent<CultRitesHandComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<CultRitesHandComponent, CultistFactoryItemSelectedMessage>(OnBloodRitesSelected);
        SubscribeLocalEvent<CultRitesHandComponent, ActivatableUIOpenAttemptEvent>(OnRitesSelectAttempt);
        SubscribeLocalEvent<CultRitesHandComponent, BeforeActivatableUIOpenEvent>(BeforeRitesSelect);
        SubscribeLocalEvent<CultRitesHandComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<CultRitesHandComponent> ent, ref ExaminedEvent args)
    {
        var user = args.Examiner;
        if (user != Transform(ent).ParentUid || !TryComp(user, out CultistComponent? cultist))
            return;
        args.PushMarkup(Loc.GetString("cult-rites-examine", ("blood", cultist.RitesBloodAmount)));
    }

    private void BeforeRitesSelect(Entity<CultRitesHandComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (_ui.TryGetUi(ent, BloodRitesUi.Key, out var bui))
            _ui.SetUiState(bui, new CultistFactoryBUIState(ent.Comp.BloodRites));
    }

    private void OnRitesSelectAttempt(Entity<CultRitesHandComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!HasComp<CultistComponent>(args.User))
            args.Cancel();
    }

    private void OnBloodRitesSelected(Entity<CultRitesHandComponent> ent, ref CultistFactoryItemSelectedMessage args)
    {
        var attachedEntity = args.Session.AttachedEntity;

        if (!TryComp(attachedEntity, out CultistComponent? cultist))
            return;

        if (!_prototypeManager.TryIndex<CultistFactoryProductionPrototype>(args.Item, out var prototype))
            return;

        var uid = attachedEntity.Value;

        if (cultist.RitesBloodAmount < prototype.BloodCost)
        {
            var message = Loc.GetString("cult-rites-no-blood", ("required", prototype.BloodCost),
                ("blood", cultist.RitesBloodAmount));
            Popup(message, uid);
            return;
        }

        Speak(uid, ent.Comp);
        Del(ent);

        var success = false;
        foreach (var entity in prototype.Item.Select(item => Spawn(item, Transform(uid).Coordinates)))
        {
            if (_handsSystem.TryPickupAnyHand(uid, entity))
            {
                success = true;
                continue;
            }

            Popup(Loc.GetString("cult-rites-no-hand"), uid);
            QueueDel(entity);
            break;
        }

        if (!success)
            return;

        cultist.RitesBloodAmount -= prototype.BloodCost;
    }

    private void OnInteract(Entity<CultRitesHandComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target)
            return;

        var puddleQuery = GetEntityQuery<PuddleComponent>();
        if (!puddleQuery.HasComp(target))
            return;

        if (!TryComp(args.User, out BloodstreamComponent? bloodstreamComponent) ||
            !TryComp(args.User, out CultistComponent? cultist))
            return;

        var xform = Transform(target);

        var entitiesInRange = _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(xform), 1.5f);

        var totalBloodAmount = FixedPoint2.Zero;

        foreach (var solutionEntity in entitiesInRange.ToList())
        {
            if (!puddleQuery.TryComp(solutionEntity, out var puddleComponent))
                continue;

            if (!_solutionSystem.TryGetSolution(solutionEntity, puddleComponent.SolutionName, out var solution))
                continue;

            var hasBlood = false;
            foreach (var solutionContent in solution.Value.Comp.Solution.Contents.ToList()
                         .Where(solutionContent => solutionContent.Reagent.Prototype == "Blood"))
            {
                hasBlood = true;
                totalBloodAmount += solutionContent.Quantity;

                _bloodstream.TryModifyBloodLevel(args.User, solutionContent.Quantity / 6f, bloodstreamComponent);
                _solutionSystem.RemoveReagent((Entity<SolutionComponent>) solution, "Blood", FixedPoint2.MaxValue);
            }

            if (hasBlood)
                Spawn("CultTileEffect", Transform(solutionEntity).Coordinates);
        }

        if (totalBloodAmount <= FixedPoint2.Zero)
            return;

        cultist.RitesBloodAmount += totalBloodAmount;

        _audio.PlayPvs(ent.Comp.SuckSound, args.User);
        _popupSystem.PopupEntity(Loc.GetString("cult-rites-message", ("blood", cultist.RitesBloodAmount)),
            args.User, args.User);

        args.Handled = true;
    }

    private void OnRitesHit(Entity<CultRitesHandComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        var target = args.HitEntities[0];
        var (uid, comp) = ent;

        QueueDel(uid);

        if (!TryComp(args.User, out CultistComponent? cultist) || !TryComp(target, out DamageableComponent? damageable))
            return;

        if (_mobState.IsDead(target))
        {
            Popup(Loc.GetString("cult-rites-dead"), args.User);
            return;
        }

        if (cultist.RitesBloodAmount <= FixedPoint2.Zero)
        {
            Popup(Loc.GetString("cult-rites-heal-no-blood"), args.User);
            return;
        }

        var damage = damageable.Damage;
        var totalDamage = damage.GetTotal();
        if (totalDamage <= FixedPoint2.Zero)
        {
            Popup(Loc.GetString("cult-rites-already-healed"), args.User);
            return;
        }

        var coef = FixedPoint2.Min(cultist.RitesBloodAmount * comp.HealModifier, totalDamage) / totalDamage;
        cultist.RitesBloodAmount =
            FixedPoint2.Max(FixedPoint2.Zero, cultist.RitesBloodAmount - totalDamage / comp.HealModifier);
        _damageable.TryChangeDamage(target, -damage * coef, true, false, damageable, args.User);
        Popup(Loc.GetString("cult-rites-after-heal", ("blood", cultist.RitesBloodAmount)), args.User);
        _audio.PlayPvs(comp.HealSound, target);
        Speak(args.User, comp);
    }

    private void OnStunHit(Entity<CultStunHandComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        var target = args.HitEntities[0];
        var (uid, comp) = ent;

        QueueDel(uid);
        Spawn("CultStunFlashEffect", Transform(target).Coordinates);
        Speak(args.User, comp);
        if (TryComp(args.User, out BloodstreamComponent? bloodstream))
            _bloodstream.TryModifyBloodLevel(args.User, -10, bloodstream, createPuddle: false);

        if (_holyWeapon.IsHoldingHolyWeapon(target))
        {
            Popup(Loc.GetString("cult-magic-holy"), args.User, PopupType.MediumCaution);
            return;
        }

        var halo = HasComp<PentagramComponent>(args.User);

        _statusEffects.TryAddStatusEffect(target, "Muted", halo ? comp.HaloMuteDuration : comp.MuteDuration, true,
            "Muted");
        _stun.TryParalyze(target, halo ? comp.HaloDuration : comp.Duration, true);
    }

    private void Popup(string msg, EntityUid user, PopupType type = PopupType.Small)
    {
        _popupSystem.PopupEntity(msg, user, user, type);
    }

    private void Speak(EntityUid user, BaseMagicHandComponent comp)
    {
        _chat.TrySendInGameICMessage(user, comp.Speech, InGameICChatType.Whisper, false);
    }
}
