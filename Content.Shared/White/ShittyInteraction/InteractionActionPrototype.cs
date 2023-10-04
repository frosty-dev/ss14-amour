using Content.Shared.Actions.ActionTypes;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.White.ShittyInteraction;

[Prototype("interactionTargetAction")]
public sealed class InteractionActionPrototype : EntityTargetAction, IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("serverEvent", serverOnly: true)]
    [NonSerialized]
    public BaseInteractionEvent? ServerEvent;

    [DataField("endEvent", serverOnly: true)]
    [NonSerialized]
    public BaseInteractionEvent? EndEvent;

    [DataField("startSound")] public SoundSpecifier? StartSound;
    [DataField("endSound")] public SoundSpecifier? EndSound;

    [DataField("messages")] public List<string> Messages = new List<string>();
    [DataField("endMessages")] public List<string> EndMessages = new List<string>();

    [DataField("interactionTime" )]
    public int InteractionTime;

    [DataField("timeout")]
    public int Timeout;

    [DataField("isCloseInteraction")] public bool IsCloseInteraction;
}
