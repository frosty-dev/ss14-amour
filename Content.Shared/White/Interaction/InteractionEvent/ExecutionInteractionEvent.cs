using Content.Shared.Actions;
using Content.Shared.White.Interaction.Interactions;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Interaction.InteractionEvent;

public sealed class ExecutionInteractionEvent : EntityTargetActionEvent
{
    public string EventName;

    public ExecutionInteractionEvent(string eventName)
    {
        EventName = eventName;
    }
}
