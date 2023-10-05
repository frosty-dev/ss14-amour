using Content.Shared.Actions;

namespace Content.Shared.White.ShittyInteraction.Events;

public sealed class ExecutionInteractionEvent : EntityTargetActionEvent
{
    public string EventName;

    public ExecutionInteractionEvent(string eventName)
    {
        EventName = eventName;
    }
}
