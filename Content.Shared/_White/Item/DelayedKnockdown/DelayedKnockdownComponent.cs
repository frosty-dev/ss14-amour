using Robust.Shared.GameStates;

namespace Content.Shared._White.Item.DelayedKnockdown;

[RegisterComponent, NetworkedComponent]
public sealed partial class DelayedKnockdownComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5);

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan KnockdownMoment = TimeSpan.MaxValue;
}
