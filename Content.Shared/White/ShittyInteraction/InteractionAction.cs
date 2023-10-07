using Content.Shared.Actions.ActionTypes;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.White.ShittyInteraction;

[Serializable, NetSerializable, Virtual]
public class InteractionAction : EntityTargetAction
{
    [DataField("serverEvent", serverOnly: true)]
    [NonSerialized]
    public BaseInteractionEvent? StartEvent;

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

    [DataField("performerRequirement")]
    public InteractionRequirement PerformerRequirement = new();
    [DataField("targetRequirement")]
    public InteractionRequirement TargetRequirement = new();

    public InteractionAction(){}

    public InteractionAction(InteractionAction toClone)
    {
        CopyFrom(toClone);
    }

    public override void CopyFrom(object objectToClone)
    {
        base.CopyFrom(objectToClone);

        if(objectToClone is not InteractionAction interactionAction)
            return;

        StartEvent = interactionAction.StartEvent;
        EndEvent = interactionAction.EndEvent;
        StartSound = interactionAction.StartSound;
        EndSound = interactionAction.EndSound;
        Messages = interactionAction.Messages;
        EndMessages = interactionAction.EndMessages;
        InteractionTime = interactionAction.InteractionTime;
        Timeout = interactionAction.Timeout;
        IsCloseInteraction = interactionAction.IsCloseInteraction;
        PerformerRequirement = interactionAction.PerformerRequirement;
        TargetRequirement = interactionAction.TargetRequirement;
    }

    public override object Clone()
    {
        return new InteractionAction(this);
    }
}
