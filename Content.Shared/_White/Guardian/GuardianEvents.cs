using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Guardian;

public enum GuardianSelector : byte
{
    Assasin,
    Standart,
    Charger,
    Lighting
}

[Serializable, NetSerializable]
public enum GuardianSelectorUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class GuardianSelectorBUIState : BoundUserInterfaceState
{
    public IReadOnlyCollection<string> Ids { get; set; }

    public NetEntity Target { get; set; }

    public GuardianSelectorBUIState(IReadOnlyCollection<string> ids, NetEntity target)
    {
        Ids = ids;
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed class GuardianSelectorSelectedBuiMessage : BoundUserInterfaceMessage
{
    public GuardianSelector GuardianType;
    public NetEntity Target;

    public GuardianSelectorSelectedBuiMessage(GuardianSelector guardianType, NetEntity target)
    {
        GuardianType = guardianType;
        Target = target;
    }
}

public sealed partial class ToggleGuardianPowerActionEvent : InstantActionEvent
{
}

public sealed partial class ChargerPowerActionEvent : InstantActionEvent
{
}
