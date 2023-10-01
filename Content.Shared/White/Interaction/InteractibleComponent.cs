using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameStates;

namespace Content.Shared.White.Interaction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InteractibleComponent : Component
{
    [DataField("isActive"), AutoNetworkedField]
    public bool IsActive;

    [DataField("availableInteractions"), AutoNetworkedField]
    public List<string> AvailableInteractions = new List<string>();

    [ViewVariables(VVAccess.ReadWrite), DataField("action")]
    public TargetedAction? Action;
}
