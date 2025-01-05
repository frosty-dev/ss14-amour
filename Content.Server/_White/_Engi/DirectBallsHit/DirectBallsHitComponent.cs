using Content.Shared.Standing.Systems;

namespace Content.Server._White._Engi.DirectBallsHit;

[RegisterComponent]
public sealed partial class DirectBallsHitComponent : Component
{
    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2.0f);

    [DataField]
    public TimeSpan JitterTime = TimeSpan.FromSeconds(2.0f);

    [DataField]
    public TimeSpan StutterTime = TimeSpan.FromSeconds(2.0f);

    [DataField]
    public SharedStandingStateSystem.DropHeldItemsBehavior KnockDownBehavior = SharedStandingStateSystem.DropHeldItemsBehavior.AlwaysDrop;

    [DataField]
    public bool RequireWield = true;
}
