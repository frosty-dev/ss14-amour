using Robust.Shared.GameStates;

namespace Content.Shared.White.Crawl;

[RegisterComponent, NetworkedComponent]
public sealed class CrawlComponent : Component
{
    public float SprintSpeedModifier { get; set; } = 0.4f;
    public float WalkSpeedModifier { get; set; } = 0.4f;
}
