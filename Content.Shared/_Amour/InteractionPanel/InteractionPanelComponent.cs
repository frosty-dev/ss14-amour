using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.InteractionPanel;

[RegisterComponent]
public sealed partial class InteractionPanelComponent : Component
{
    [ViewVariables] public bool IsActive = false;
    [ViewVariables] public bool IsBlocked = false;
    [ViewVariables] public ProtoId<InteractionPrototype> CurrentAction;
    [ViewVariables] public TimeSpan Timeout;
    [ViewVariables] public Entity<InteractionPanelComponent>? CurrentPartner;

    [DataField] public List<ProtoId<InteractionPrototype>> ActionPrototypes = new();
}

