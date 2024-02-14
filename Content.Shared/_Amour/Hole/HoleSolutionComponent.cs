using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.Hole;

[RegisterComponent]
public sealed partial class HoleSolutionComponent : Component
{
    // Funny cunt
    [DataField] public ProtoId<ReagentPrototype> SubstanceName = "JuiceOrange";

    [ViewVariables] public Solution Solution = default!;
    [ViewVariables]
    public TimeSpan NextGenerationTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField]
    public float SubstanceHoldCount = 5;

    [DataField]
    public float SubstanceGenerationCount = 0.25f;

    public static string SlotName = "Funny";
}
