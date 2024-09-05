namespace Content.Shared._Honk.Cunt;

[RegisterComponent]
public sealed partial class CuntableComponent : Component
{
    public const string CuntSolutionName = "cunt";

    [ViewVariables] public TimeSpan NextRegenTime = TimeSpan.Zero;
    [ViewVariables] public TimeSpan Duration = TimeSpan.FromSeconds(1);
}
