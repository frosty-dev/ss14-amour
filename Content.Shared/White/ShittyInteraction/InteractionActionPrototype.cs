using Robust.Shared.Prototypes;

namespace Content.Shared.White.ShittyInteraction;

[Prototype("interactionTargetAction")]
public sealed class InteractionActionPrototype : InteractionAction, IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;
}
