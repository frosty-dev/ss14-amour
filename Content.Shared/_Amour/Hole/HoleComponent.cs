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
    [ViewVariables] public NetEntity? Parent;

    [DataField] public string HoleName = "";
    [DataField] public List<string> HoleNotVisibleIn = new();

    // Father can be in mother, its like in audiofil shit
    [DataField] public HoleType HoleType = HoleType.Flat;

    [DataField("sprite")] public string? RsiPath;
    [DataField] public List<PrototypeLayerData> Layers = new();

    // this shit just for sprite prefix like state: dildo_FRONT
    [DataField] public List<HolePrefix> Prefixes = new();
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class HolePrefix
{
    [DataField]
    public string Layer;
    [DataField]
    public string Prefix;
}

public enum HoleType : byte
{
    Father,
    Flat,
    Mother
}
