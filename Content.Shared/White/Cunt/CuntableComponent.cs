using Content.Shared.Chemistry.Components;

namespace Content.Shared.White.Cunt;

[RegisterComponent]
public sealed class CuntableComponent : Component
{
    public const string CuntSolutionName = "cunt";

    [ViewVariables] public TimeSpan NextRegenTime = TimeSpan.Zero;
    [ViewVariables] public TimeSpan Duration = TimeSpan.FromSeconds(1);
}
