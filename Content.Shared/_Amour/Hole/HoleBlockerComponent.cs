using Robust.Shared.GameStates;

namespace Content.Shared._Amour.Hole;

[RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
public sealed partial class HoleBlockerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public NetEntity Equipee = NetEntity.Invalid;
}
