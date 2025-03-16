namespace Content.Server._White.FillIDCard;

/// <summary>
/// WD.
/// </summary>
[RegisterComponent]
public sealed partial class FillIDCardComponent : Component
{
    [DataField]
    public bool IsContractor = false;
}
