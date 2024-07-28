namespace Content.Server._White.Cult.Items.Components;

[RegisterComponent]
public sealed partial class CultStunHandComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(16);

    [DataField]
    public TimeSpan HaloDuration = TimeSpan.FromSeconds(1.5);

    [DataField]
    public TimeSpan MuteDuration = TimeSpan.FromSeconds(12);

    [DataField]
    public TimeSpan HaloMuteDuration = TimeSpan.FromSeconds(1);

    [DataField]
    public string Speech = "Fuu ma'jin!";
}
