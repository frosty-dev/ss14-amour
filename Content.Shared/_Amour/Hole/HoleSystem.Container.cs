using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.Hole;

public abstract partial class SharedHoleSystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
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

    public void AddHole(Entity<HoleContainerComponent?> entity, EntProtoId protoId, Color? color = null)
    {
        if (!_prototypeManager.TryIndex<EntityPrototype>(protoId, out _))
        {
            Log.Error(protoId + " NOT EXIST YOU BASTARD!");
            return;
        }
        if (!Resolve(entity.Owner, ref entity.Comp,logMissing:false))
           entity.Comp = EnsureComp<HoleContainerComponent>(entity.Owner);

        var spawned = Spawn(protoId);
        if (!TryComp<HoleComponent>(spawned, out var component))
        {
            QueueDel(spawned);
            return;
        }

        Log.Debug("ADDED " + protoId);
        component.Layers[0].Color = color;

        _containerSystem.Insert(spawned, entity.Comp.Slot);
    }
}
