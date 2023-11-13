using Robust.Shared.GameStates;

namespace Content.Shared.White.Crawl;

[RegisterComponent, NetworkedComponent]
public sealed class CrawlComponent : Component
{
    [ViewVariables] public float SpringSpeed = 1;
    [ViewVariables] public float WalkSpeed = 1;
}
