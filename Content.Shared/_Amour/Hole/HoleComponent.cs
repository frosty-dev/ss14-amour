using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Item;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Amour.Hole;

[RegisterComponent]
public sealed partial class HoleComponent : Component
{
    [ViewVariables] public NetEntity Parent;

    // Father can be in mother, its like in audiofil shit
    [DataField] public HoleType HoleType = HoleType.Flat;

    [DataField("sprite")] public string? RsiPath;
    [DataField] public List<PrototypeLayerData> FrontLayer = new();
    [DataField] public List<PrototypeLayerData> BehindLayer = new();
}

public enum HoleType : byte
{
    Father,
    Flat,
    Mother
}
