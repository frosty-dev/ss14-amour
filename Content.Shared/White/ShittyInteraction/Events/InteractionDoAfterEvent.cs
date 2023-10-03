using Content.Shared.DoAfter;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Shared.Serialization;

namespace Content.Shared.White.ShittyInteraction.Events;

[Serializable, NetSerializable]
public sealed class InteractionDoAfterEvent : SimpleDoAfterEvent
{
    public BaseInteractionEvent? EndEvent;
    public int Timeout;

    public InteractionDoAfterEvent(BaseInteractionEvent? endEvent, int timeout)
    {
        EndEvent = endEvent;
        Timeout = timeout;
    }
}
