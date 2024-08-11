namespace Content.Server._White.Cult.Items.Components;

[RegisterComponent]
public sealed partial class CultStunHandComponent : BaseMagicHandComponent
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(16);

    [DataField]
    public TimeSpan MuteDuration = TimeSpan.FromSeconds(12);

    [DataField]
    public float PentagramDurationMultiplier = 0.1f;
}
