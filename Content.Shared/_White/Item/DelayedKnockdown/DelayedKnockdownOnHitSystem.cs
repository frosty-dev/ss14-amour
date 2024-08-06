using Content.Shared.Damage.Events;
using Content.Shared.Jittering;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Shared._White.Item.DelayedKnockdown;

public sealed class DelayedKnockdownOnHitSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DelayedKnockdownOnHitComponent, StaminaMeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<DelayedKnockdownOnHitComponent> ent, ref StaminaMeleeHitEvent args)
    {
        var jitterTime = ent.Comp.JitterTime;
        var stutterTime = ent.Comp.StutterTime;
        var delay = ent.Comp.Delay;
        var knockdownTime = ent.Comp.KnockdownTime;
        foreach (var (uid, _) in args.HitList)
        {
            if (!TryComp(uid, out StatusEffectsComponent? statusEffects))
                continue;

            if (!TryComp(uid, out StandingStateComponent? standingState) || !standingState.CanLieDown)
                continue;

            if (jitterTime > TimeSpan.Zero)
                _jitter.DoJitter(uid, jitterTime, true, status: statusEffects);
            if (stutterTime > TimeSpan.Zero)
                _stutter.DoStutter(uid, stutterTime, true, statusEffects);
            if (delay <= TimeSpan.Zero)
            {
                _stun.TryKnockdown(uid, knockdownTime, true, statusEffects);
                continue;
            }
            if (HasComp<KnockedDownComponent>(uid))
                continue;
            var delayedKnockdown = EnsureComp<DelayedKnockdownComponent>(uid);
            delayedKnockdown.KnockdownTime = TimeSpan.FromSeconds(Math.Max(knockdownTime.TotalSeconds,
                delayedKnockdown.KnockdownTime.TotalSeconds));
            var knockdownMoment = _timing.CurTime + delay;
            if (knockdownMoment < delayedKnockdown.KnockdownMoment)
                delayedKnockdown.KnockdownMoment = knockdownMoment;
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DelayedKnockdownComponent>();
        while (query.MoveNext(out var uid, out var delayedKnockdown))
        {
            if (delayedKnockdown.KnockdownMoment > _timing.CurTime)
                continue;

            _stun.TryKnockdown(uid, delayedKnockdown.KnockdownTime, true);
            RemCompDeferred<DelayedKnockdownComponent>(uid);
        }
    }
}
