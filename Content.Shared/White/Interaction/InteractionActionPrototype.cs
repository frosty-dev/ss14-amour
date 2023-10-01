using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.Interaction;

[Prototype("interactionTargetAction")]
public sealed class InteractionActionPrototype : EntityTargetAction, IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("serverEvent", serverOnly: true)]
    public Interactions.BaseInteractionEvent ServerEvent = default!;

    [DataField("interactionTime" )]
    public int InteractionTime;

    [DataField("isCloseInteraction")] public bool IsCloseInteraction;
}
