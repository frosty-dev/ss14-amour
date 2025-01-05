using Content.Shared.Damage.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Wieldable.Components;

namespace Content.Shared._White.Item.KnockDownOnHit;

public sealed class KnockDownOnHitSystem : EntitySystem
{

    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedItemToggleSystem _itemToggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KnockDownOnHitComponent, StaminaDamageOnHitAttemptEvent>(OnStaminaHitAttempt);
        SubscribeLocalEvent<KnockDownOnHitComponent, MeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<KnockDownOnHitComponent> ent, ref MeleeHitEvent args)
    {
        var time = ent.Comp.KnockdownTime;
        if (time <= TimeSpan.Zero)
            return;

        if (ent.Comp.RequireWield)
        {
            if (!TryComp<WieldableComponent>(args.Weapon, out var weapon))
                return;

            if (!weapon.Wielded)
                return;
        }

        foreach (var uid in args.HitEntities)
        {
            _stun.TryKnockdown(uid, time, true, behavior: ent.Comp.KnockDownBehavior);
        }
    }

    private void OnStaminaHitAttempt(Entity<KnockDownOnHitComponent> entity, ref StaminaDamageOnHitAttemptEvent args)
    {
        if (!_itemToggle.IsActivated(entity.Owner))
            args.Cancelled = true;
    }
}
