using Content.Shared._Amour.Hole;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._Amour.Hole;

public sealed partial class HoleSystem
{
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    private void InitializeContainer()
    {
        SubscribeLocalEvent<HoleContainerComponent,ComponentInit>(OnContainerInit);
    }

    private void OnContainerInit(EntityUid uid, HoleContainerComponent component, ComponentInit args)
    {
        component.Slot = _containerSystem.EnsureContainer<Container>(uid, HoleContainerComponent.SlotName);
        foreach (var holePrototype in component.HolePrototypes)
        {
            Log.Debug("ADDED " + holePrototype);
            _containerSystem.Insert(Spawn(holePrototype), component.Slot);
        }
    }
}
