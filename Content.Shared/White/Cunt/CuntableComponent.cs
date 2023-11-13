using Content.Shared.Chemistry.Components;

namespace Content.Shared.White.Cunt;

[RegisterComponent]
public sealed class CuntableComponent : Component
{
    public const string CuntSolutionName = "cunt";

    [ViewVariables]
    public TimeSpan NextRegenTime = TimeSpan.Zero;

    [DataField("duration")]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField("cuntCount", readOnly: true)]
    public float CuntCount = 5;

    [DataField("cuntGetCount")]
    public float CuntGetCount = 0.25f;
}
