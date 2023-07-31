using Content.Shared.Movement.Components;

namespace Content.Shared.White.Crawl;

[RegisterComponent]
public sealed class CrawlComponent : Component
{
    [ViewVariables] public float SpringSpeed = 1;
    [ViewVariables] public float WalkSpeed = 1;
}
