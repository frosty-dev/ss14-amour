namespace Content.Shared._Amour.RoleplayInfo;

[RegisterComponent]
public sealed partial class RoleplayInfoComponent : Component
{
    [DataField] public List<RoleplayInfo> Data = new();
}
