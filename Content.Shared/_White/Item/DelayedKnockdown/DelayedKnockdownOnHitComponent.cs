using Robust.Shared.GameStates;

namespace Content.Shared._White.Item.DelayedKnockdown;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DelayedKnockdownOnHitComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2);

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5);

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan StutterTime = TimeSpan.FromSeconds(16);

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan JitterTime = TimeSpan.FromSeconds(40);
}
