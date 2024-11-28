using Robust.Shared.GameStates;

namespace Content.Shared._White.Wizard.Appearance;

[RegisterComponent, NetworkedComponent]
public sealed partial class WizardAppearanceComponent : Component
{
    [DataField] public int MinAge = 90;

    [DataField] public int MaxAge = 170;

    [DataField] public string Hair = "WizardHair";

    [DataField] public string FacialHair = "WizardFacialHair";

    [DataField] public string Color = "WizardHairColor";

    [DataField] public string Name = "WizardNames";
}
