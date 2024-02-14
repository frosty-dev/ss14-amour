using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.Hole;

[RegisterComponent]
public sealed partial class HoleContainerComponent : Component
{
    public const string SlotName = "Funny";
    [ViewVariables] public Container Slot = default!;
    [DataField] public List<EntProtoId> HolePrototypes = new();
}
