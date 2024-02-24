using Robust.Shared.GameStates;

namespace Content.Shared._Amour.Crawl;

[RegisterComponent, NetworkedComponent]
public sealed partial class CrawlComponent : Component
{
    public float SprintSpeedModifier { get; set; } = 0.4f;
    public float WalkSpeedModifier { get; set; } = 0.4f;
}
