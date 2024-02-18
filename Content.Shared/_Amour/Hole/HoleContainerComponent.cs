using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.Hole;

[RegisterComponent,NetworkedComponent]
public sealed partial class HoleContainerComponent : Component
{
    public const string SlotName = "Funny";
    [ViewVariables] public Container Slot = default!;
    [ViewVariables] public NetEntity? MainHole;
    [DataField] public List<EntProtoId> HolePrototypes = new();
    [DataField] public bool UseHumanGenitalLayers = false;
}
