namespace Content.Shared._Amour.CustomHeight;

[RegisterComponent]
public sealed partial class CustomHeightComponent : Component
{
    [DataField]
    public float Min = 0.89f;
    [DataField]
    public float Max = 1.1f;
    [DataField]
    public float Starting = 1f;
    [DataField]
    public bool Random = true;
}
