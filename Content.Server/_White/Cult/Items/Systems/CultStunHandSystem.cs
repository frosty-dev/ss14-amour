using Content.Server._White.Cult.Items.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._White.Chaplain;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._White.Cult.Items.Systems;

public sealed class CultStunHandSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly HolyWeaponSystem _holyWeapon = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultStunHandComponent, MeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<CultStunHandComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        var target = args.HitEntities[0];
        var (uid, comp) = ent;

        QueueDel(uid);
        Spawn("CultStunFlashEffect", Transform(target).Coordinates);
        _chat.TrySendInGameICMessage(args.User, comp.Speech, InGameICChatType.Whisper, false);
        if (TryComp(args.User, out BloodstreamComponent? bloodstream))
            _bloodstream.TryModifyBloodLevel(args.User, -10, bloodstream, createPuddle: false);

        if (_holyWeapon.IsHoldingHolyWeapon(target))
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-magic-holy"), args.User, args.User, PopupType.MediumCaution);
            return;
        }

        var halo = HasComp<PentagramComponent>(args.User);

        _statusEffects.TryAddStatusEffect(target, "Muted", halo ? comp.HaloMuteDuration : comp.MuteDuration, true,
            "Muted");
        _stun.TryParalyze(target, halo ? comp.HaloDuration : comp.Duration, true);
    }
}
