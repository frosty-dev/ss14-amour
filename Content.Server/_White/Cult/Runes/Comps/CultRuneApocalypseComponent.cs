namespace Content.Server._White.Cult.Runes.Comps;

[RegisterComponent]
public sealed partial class CultRuneApocalypseComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("rangeTarget")]
    public float RangeTarget = 1.2f;

    [ViewVariables(VVAccess.ReadWrite), DataField("summonMinCount")]
    public uint SummonMinCount = 10;
}
