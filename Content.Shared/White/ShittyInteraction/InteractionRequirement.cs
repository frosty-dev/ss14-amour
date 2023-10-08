using Robust.Shared.Serialization;

namespace Content.Shared.White.ShittyInteraction;

[Serializable,NetSerializable]
[DataDefinition]
public sealed class InteractionRequirement
{
    [DataField("ass")]
    public bool AssRequired;
    [DataField("penis")]
    public bool PenisRequired;
    [DataField("vagina")]
    public bool VaginaRequired;
}
