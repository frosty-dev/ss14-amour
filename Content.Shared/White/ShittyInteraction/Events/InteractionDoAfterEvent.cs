using Content.Shared.DoAfter;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Shared.Serialization;

namespace Content.Shared.White.ShittyInteraction.Events;

[Serializable, NetSerializable]
public sealed class InteractionDoAfterEvent : SimpleDoAfterEvent
{
    public InteractionAction Action;

    public InteractionDoAfterEvent(InteractionAction action)
    {
        Action = action;
    }
}
