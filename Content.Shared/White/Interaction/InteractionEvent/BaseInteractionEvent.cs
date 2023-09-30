using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Interaction.InteractionEvent;

public sealed class BaseInteractionEvent : EntityTargetActionEvent
{
    [NonSerialized]
    [DataField("event")]
    public EntityTargetActionEvent Event;

    public BaseInteractionEvent(EntityTargetActionEvent @event)
    {
        Event = @event;
    }
}
