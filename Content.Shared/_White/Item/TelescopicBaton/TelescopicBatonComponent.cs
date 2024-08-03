namespace Content.Shared._White.Item.TelescopicBaton;

[RegisterComponent]
public sealed partial class TelescopicBatonComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(1.5f);
}
