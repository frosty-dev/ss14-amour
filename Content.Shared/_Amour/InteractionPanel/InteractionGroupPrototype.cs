using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.InteractionPanel;

[Prototype("interactionGroup")]
public sealed class InteractionGroupPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [DataField] public Color Color;
    [DataField] public int Priority;
}
