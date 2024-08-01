using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Item.PseudoItem;

public abstract class SharedPseudoItemSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PseudoItemComponent, GettingPickedUpAttemptEvent>(OnGettingPickedUpAttempt);
        SubscribeLocalEvent<PseudoItemComponent, AttackAttemptEvent>(OnAttackAttempt);
    }

    private void OnAttackAttempt(Entity<PseudoItemComponent> ent, ref AttackAttemptEvent args)
    {
        if (ent.Comp.Active)
            args.Cancel();
    }

    private void OnGettingPickedUpAttempt(Entity<PseudoItemComponent> ent, ref GettingPickedUpAttemptEvent args)
    {
        args.Cancel();
        OnGettingPickedUp(ent, args);
    }

    protected virtual void OnGettingPickedUp(Entity<PseudoItemComponent> ent, GettingPickedUpAttemptEvent args) {}
}

public sealed class PseudoItemInteractEvent(EntityUid used, EntityUid user, EntityUid virtualItem)
    : EntityEventArgs
{
    public EntityUid Used { get; } = used;
    public EntityUid User { get; } = user;
    public EntityUid VirtualItem { get; } = virtualItem;
}

[Serializable, NetSerializable]
public sealed partial class PseudoItemInsertEvent : SimpleDoAfterEvent
{
}
