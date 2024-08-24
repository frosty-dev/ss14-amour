using Content.Shared._White.Cult.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Cult.GameRule;

[RegisterComponent]
public sealed partial class CultRuleComponent : Component
{
    [DataField]
    public ProtoId<EntityPrototype> ReaperPrototype = "ReaperConstruct";

    [ViewVariables(VVAccess.ReadOnly), DataField("tileId")]
    public string CultFloor = "CultFloor";

    [DataField]
    public Color EyeColor = Color.FromHex("#f80000");

    public ProtoId<ReagentPrototype> HolyWaterReagent = "Holywater";

    [DataField]
    public int ReadEyeThreshold = 5;

    [DataField]
    public int PentagramThreshold = 8;

    /// <summary>
    ///     Players who played as an cultist at some point in the round.
    /// </summary>
    public Dictionary<string, string> CultistsCache = new();

    public EntityUid? CultTarget;

    public List<CultistComponent> CurrentCultists = new();

    public List<ConstructComponent> Constructs = new();

    public CultWinCondition WinCondition = CultWinCondition.Draw;

    public CultStage Stage = CultStage.Normal;
}

public enum CultWinCondition : byte
{
    Draw,
    Win,
    Failure,
}

public enum CultStage : byte
{
    Normal,
    RedEyes,
    Pentagram,
}

public sealed class CultNarsieSummoned : EntityEventArgs;
