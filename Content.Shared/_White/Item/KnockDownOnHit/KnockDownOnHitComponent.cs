using Content.Shared.Standing.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Item.KnockDownOnHit;

[RegisterComponent, NetworkedComponent]
public sealed partial class KnockDownOnHitComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(1.5f);

    [DataField]
    public SharedStandingStateSystem.DropHeldItemsBehavior? KnockDownBehavior;

    [DataField]
    public bool RequireWield;
}
