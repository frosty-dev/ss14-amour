using Robust.Shared.Containers;

namespace Content.Shared._Amour.Hole;

public abstract partial class SharedHoleSystem : EntitySystem
{
    public override void Initialize()
    {
        InitializeContainer();
        SubscribeLocalEvent<HoleComponent,EntGotInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<HoleComponent,EntGotRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnRemoved(EntityUid uid, HoleComponent component, EntGotRemovedFromContainerMessage args)
    {
        component.Parent = null;
    }

    private void OnInsert(EntityUid uid, HoleComponent component, EntGotInsertedIntoContainerMessage args)
    {
        component.Parent = GetNetEntity(args.Container.Owner);
    }

    public virtual void Exide(Entity<HoleComponent?> entity, bool value = true)
    {
        if(!Resolve(entity.Owner,ref entity.Comp)) return;
        entity.Comp.IsExcited = value;
    }
}
