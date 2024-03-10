namespace Content.Shared._Amour.CustomHeight;

[RegisterComponent]
public sealed partial class CustomHeightComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Min = 0.94f;
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Max = 1.06f;
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float Starting = 1f;
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool Random = true;
}
