using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Wieldable.Components;
using Content.Shared.Jittering;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Standing;
using Content.Shared.Electrocution;
using Content.Shared.Popups;
using Content.Shared._White.Implants.NeuroControl;
using Robust.Shared.Timing;
using Content.Server.Chat.Systems;
using Content.Server._White.Mood;

namespace Content.Server._White._Engi.DirectBallsHit;

public sealed class DirectBallsHitSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private readonly SharedElectrocutionSystem _electrocution = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MoodSystem _mood = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DirectBallsHitComponent, MeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<DirectBallsHitComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.RequireWield)
        {
            if (!TryComp<WieldableComponent>(args.Weapon, out var weapon))
                return;

            if (!weapon.Wielded)
                return;
        }

        foreach (var uid in args.HitEntities)
        {
            if (TryComp<MoodComponent>(uid, out var mood))
            {
                _popupSystem.PopupEntity(
                    Loc.GetString("direct-balls-hit", ("uid", uid)),
                    uid,
                    PopupType.SmallCaution);

                _mood.ApplyEffect(uid, mood, "GotHitInTheBalls");
            }

            Timer.Spawn(TimeSpan.FromSeconds(0.5f), () => _chat.TryEmoteWithChat(uid, "Scream"));

            if (HasComp<NeuroStabilizationComponent>(uid))
            {
                _electrocution.TryDoElectrocution(uid, null, 30, TimeSpan.FromSeconds(1), false, 0.5f, null, true);
                continue;
            }

            if (TryComp(uid, out StandingStateComponent? standingState) && standingState.CanLieDown)
                _stun.TryKnockdown(uid, ent.Comp.KnockdownTime, true, behavior: ent.Comp.KnockDownBehavior);

            if (TryComp(uid, out StatusEffectsComponent? statusEffects))
            {
                _jitter.DoJitter(uid, ent.Comp.JitterTime, true, status: statusEffects);
                _stutter.DoStutter(uid, ent.Comp.StutterTime, true, statusEffects);
            }
        }
    }
}
