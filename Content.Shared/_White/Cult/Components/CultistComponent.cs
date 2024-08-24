using System.Threading;
using Content.Shared.FixedPoint;
using Content.Shared.Mind;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Cult.Components;

/// <summary>
/// This is used for tagging a mob as a cultist.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CultistComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("holyConvertTime")]
    public float HolyConvertTime = 15f;

    public CancellationTokenSource? HolyConvertToken;

    [AutoNetworkedField]
    public List<NetEntity?> SelectedEmpowers = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 RitesBloodAmount = FixedPoint2.Zero;

    public static string SummonCultDaggerAction = "InstantActionSummonCultDagger";

    public static string BloodRitesAction = "InstantActionBloodRites";

    public static string EmpPulseAction = "InstantActionEmpPulse";

    public static string ConcealPresenceAction = "InstantActionConcealPresence";

    public static string CultTwistedConstructionAction = "ActionCultTwistedConstruction";

    public static string CultTeleportAction = "ActionCultTeleport";

    public static string CultSummonCombatEquipmentAction = "ActionCultSummonCombatEquipment";

    public static string CultStunAction = "InstantActionCultStun";

    public static string CultShadowShacklesAction = "ActionCultShadowShackles";

    public static List<string> CultistActions = new()
    {
        SummonCultDaggerAction, BloodRitesAction, CultTwistedConstructionAction, CultTeleportAction,
        CultSummonCombatEquipmentAction, CultStunAction, EmpPulseAction, ConcealPresenceAction, CultShadowShacklesAction
    };

    [ViewVariables, NonSerialized]
    public Entity<BloodSpearComponent>? BloodSpear;

    [ViewVariables, NonSerialized]
    public EntityUid? BloodSpearActionEntity;

    [ViewVariables, NonSerialized]
    public Entity<MindComponent>? OriginalMind;
}
