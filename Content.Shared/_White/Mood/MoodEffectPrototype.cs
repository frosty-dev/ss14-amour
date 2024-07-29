using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared._White.Mood;

[Prototype]
public sealed class MoodEffectPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public string Description = string.Empty;

    [DataField(customTypeSerializer: typeof(EnumSerializer), required: true)]
    public Enum MoodChange = default!;

    [DataField] public bool Positive;

    [DataField] public int Timeout;

    [DataField] public bool Hidden;

    // If mob already has effect of the same category, the new one will replace the old one.
    [DataField] public string? Category;
}
