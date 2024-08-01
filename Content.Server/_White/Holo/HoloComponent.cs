namespace Content.Server._White.Holo;

[RegisterComponent]
public sealed partial class HoloComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public EntityUid? Sign;
}
