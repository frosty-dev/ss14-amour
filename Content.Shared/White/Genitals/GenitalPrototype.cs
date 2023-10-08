using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Shared.White.Genitals;

[Prototype("genital")]
public sealed class GenitalPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField("sex",required:true)] public Gender Sex = default!;
}
