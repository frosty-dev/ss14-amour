using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.Hole;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class Genital
{
    public Genital(EntProtoId genitalId, Color? color)
    {
        GenitalId = genitalId;
        Color = color;
    }

    [DataField]
    public EntProtoId GenitalId { get; private set; }
    [DataField]
    public Color? Color { get; private set; }
}
