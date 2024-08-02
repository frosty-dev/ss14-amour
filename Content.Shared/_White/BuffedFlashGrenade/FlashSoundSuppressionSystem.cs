using Content.Shared.Inventory;
using Content.Shared.Stunnable;

namespace Content.Shared._White.BuffedFlashGrenade;

public sealed class FlashSoundSuppressionSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlashSoundSuppressionComponent, InventoryRelayedEvent<GetFlashbangedEvent>>(
            OnGetFlashbanged);
    }

    private void OnGetFlashbanged(Entity<FlashSoundSuppressionComponent> ent,
        ref InventoryRelayedEvent<GetFlashbangedEvent> args)
    {
        args.Args.MaxRange = MathF.Min(args.Args.MaxRange, ent.Comp.MaxRange);
    }

    public void Stun(EntityUid target, float stunDuration, float knockdownDuration, float distance, float range)
    {
        if (TryComp<FlashSoundSuppressionComponent>(target, out var suppression))
            range = MathF.Min(range, suppression.MaxRange);

        var ev = new GetFlashbangedEvent();
        ev.MaxRange = range;
        RaiseLocalEvent(target, ev);
        range = MathF.Min(range, ev.MaxRange);

        if (range <= 0f)
            return;
        if (distance < 0f)
            distance = 0f;
        if (distance > range)
            return;

        var knockdownTime = float.Lerp(knockdownDuration, 0f, distance / range);
        if (knockdownTime > 0f)
            _stunSystem.TryKnockdown(target, TimeSpan.FromSeconds(knockdownTime), true);

        var stunTime = float.Lerp(stunDuration, 0f, distance / range);
        if (stunTime > 0f)
            _stunSystem.TryStun(target, TimeSpan.FromSeconds(stunTime), true);
    }
}

public sealed class GetFlashbangedEvent : EntityEventArgs, IInventoryRelayEvent
{
    public float MaxRange = 7f;

    public SlotFlags TargetSlots => SlotFlags.EARS | SlotFlags.HEAD;
}
