using Content.Shared.DoAfter;
using Content.Shared.White.Interaction.Interactions;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Interaction.InteractionEvent;

[Serializable, NetSerializable]
public sealed class InteractionDoAfterEvent : SimpleDoAfterEvent
{
    public BaseInteractionEvent? EndEvent;

    public InteractionDoAfterEvent(BaseInteractionEvent? endEvent)
    {
        EndEvent = endEvent;
    }
}
