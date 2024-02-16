using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.Hole;

[Prototype("genitalsGroup")]
public sealed class GenitalsGroupPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [DataField(required:true)] public List<EntProtoId> Prototypes = default!;
}
