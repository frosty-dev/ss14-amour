namespace Content.Shared._White.Item.Telebaton;

[RegisterComponent]
public sealed partial class TelebatonComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(1.5f);
}
