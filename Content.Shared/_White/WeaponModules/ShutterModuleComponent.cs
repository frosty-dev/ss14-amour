namespace Content.Shared._White.WeaponModules;

[RegisterComponent]
public sealed partial class ShutterModuleComponent : BaseModuleComponent
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public string Tag;
}
