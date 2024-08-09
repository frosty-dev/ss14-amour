namespace Content.Shared._White.Item.DelayedKnockdown;

[RegisterComponent]
public sealed partial class DelayedKnockdownOnHitComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan Delay = TimeSpan.FromSeconds(2);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan StutterTime = TimeSpan.FromSeconds(16);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan JitterTime = TimeSpan.FromSeconds(40);
}
