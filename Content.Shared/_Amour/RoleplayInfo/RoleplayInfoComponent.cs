namespace Content.Shared._Amour.RoleplayInfo;

[RegisterComponent]
public sealed partial class RoleplayInfoComponent : Component
{
    [ViewVariables] public List<RoleplayInfo> Data { get; set; } = [];
}
