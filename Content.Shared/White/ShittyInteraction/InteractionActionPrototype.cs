using Content.Shared.Actions.ActionTypes;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Shared.Prototypes;

namespace Content.Shared.White.ShittyInteraction;

[Prototype("interactionTargetAction")]
public sealed class InteractionActionPrototype : EntityTargetAction, IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("serverEvent", serverOnly: true, required:true)]
    [NonSerialized]
    public BaseInteractionEvent ServerEvent = default!;

    [DataField("endEvent", serverOnly: true)]
    [NonSerialized]
    public BaseInteractionEvent? EndEvent;


    [DataField("interactionTime" )]
    public int InteractionTime;

    [DataField("isCloseInteraction")] public bool IsCloseInteraction;
}
