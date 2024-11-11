using Robust.Shared.Serialization;

namespace Content.Shared._Amour.RoleplayInfo;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class RoleplayInfo
{
    [ViewVariables] public string Name;
    [ViewVariables] public RoleplaySelection RoleplaySelection;
    public RoleplayInfo(string name, RoleplaySelection roleplaySelection)
    {
        Name = name;
        RoleplaySelection = roleplaySelection;
    }
}

public enum RoleplaySelection
{
    No = 0,
    Maybe = 1,
    Yes = 2
}
