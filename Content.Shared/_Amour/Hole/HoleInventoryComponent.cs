using Content.Shared.Item;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.Hole;

[RegisterComponent]
public sealed partial class HoleInventoryComponent : Component
{
    public const string SlotName = "Funny";

    // Funny slot
    [DataField] public ProtoId<ItemSizePrototype> Size = "Small";
    [DataField] public ProtoId<ItemSizePrototype> MaxSize = "Normal";

    [ViewVariables] public ContainerSlot Slot = default!;
}
