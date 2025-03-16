namespace Content.Server._White.ContractorIDCard;

/// <summary>
/// WD.
/// </summary>
[RegisterComponent]
public sealed partial class ContractorIDCardComponent : Component
{
    [DataField]
    public string Details = string.Empty;
}
