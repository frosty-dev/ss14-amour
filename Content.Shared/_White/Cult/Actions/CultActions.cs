using Content.Shared.Actions;
using Content.Shared.Magic;

namespace Content.Shared._White.Cult.Actions;

public sealed partial class CultTwistedConstructionActionEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField("speech")]
    public string? Speech { get; private set; }
}

public sealed partial class CultSummonDaggerActionEvent : InstantActionEvent, ISpeakSpell
{
    [DataField("speech")]
    public string? Speech { get; private set; }
}

public sealed partial class CultStunActionEvent : InstantActionEvent
{
}

public sealed partial class CultTeleportTargetActionEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField("speech")]
    public string? Speech { get; private set; }
}

public sealed partial class CultElectromagneticPulseInstantActionEvent : InstantActionEvent, ISpeakSpell
{
    [DataField("speech")]
    public string? Speech { get; private set; }
}

public sealed partial class CultShadowShacklesTargetActionEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField("speech")]
    public string? Speech { get; private set; }
}

public sealed partial class CultSummonCombatEquipmentTargetActionEvent : EntityTargetActionEvent
{
}

[Virtual]
public partial class CultConcealPresenceInstantActionEvent : InstantActionEvent, ISpeakSpell
{
    [DataField("speech")]
    public string? Speech { get; private set; }
}

public sealed partial class CultConcealInstantActionEvent : CultConcealPresenceInstantActionEvent
{
}

public sealed partial class CultRevealInstantActionEvent : CultConcealPresenceInstantActionEvent
{
}

public sealed partial class CultBloodRitesInstantActionEvent : InstantActionEvent, ISpeakSpell
{
    [DataField("speech")]
    public string? Speech { get; private set; }
}

public sealed partial class CultBloodSpearRecallInstantActionEvent : InstantActionEvent
{
}
