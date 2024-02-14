using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.Hole;

public abstract partial class SharedHoleSystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    public void InitializeContainer()
    {
        SubscribeLocalEvent<HoleContainerComponent,ComponentInit>(OnContainerInit);
    }

    private void OnContainerInit(EntityUid uid, HoleContainerComponent component, ComponentInit args)
    {
        component.Slot = _containerSystem.EnsureContainer<Container>(uid, HoleContainerComponent.SlotName);
        foreach (var protoId in component.HolePrototypes)
        {
            AddHole(new Entity<HoleContainerComponent?>(uid,component),protoId);
        }
    }

    public void AddHole(Entity<HoleContainerComponent?> entity, EntProtoId protoId)
    {
        if (!Resolve(entity.Owner, ref entity.Comp))
            return;
        //entity.Comp = EnsureComp<HoleContainerComponent>(entity.Owner);

        Log.Debug("ADDED " + protoId);
        _containerSystem.Insert(Spawn(protoId), entity.Comp.Slot);
    }
}
