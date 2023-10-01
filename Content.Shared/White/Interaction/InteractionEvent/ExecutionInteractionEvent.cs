using Content.Shared.Actions;
using Content.Shared.White.Interaction.Interactions;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Interaction.InteractionEvent;

public sealed class ExecutionInteractionEvent : EntityTargetActionEvent
{
    public readonly BaseInteractionEvent Event;
    public readonly int InteractionTime;
    public readonly bool IsCloseInteraction;

    public ExecutionInteractionEvent(BaseInteractionEvent @event, int interactionTime, bool isCloseInteraction)
    {
        Event = @event;
        InteractionTime = interactionTime;
        IsCloseInteraction = isCloseInteraction;
    }
}
