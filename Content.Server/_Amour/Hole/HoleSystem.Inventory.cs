using Content.Shared._Amour.Hole;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._Amour.Hole;

public sealed partial class HoleSystem
{
    [Dependency] private readonly ContainerSystem _container = default!;

    private void InitializeInventory()
    {
        SubscribeLocalEvent<HoleInventoryComponent,ComponentInit>(OnInventoryInit);
    }

    private void OnInventoryInit(EntityUid uid, HoleInventoryComponent component, ComponentInit args)
    {
        component.Slot = _container.EnsureContainer<ContainerSlot>(uid,HoleInventoryComponent.SlotName);
    }
}
