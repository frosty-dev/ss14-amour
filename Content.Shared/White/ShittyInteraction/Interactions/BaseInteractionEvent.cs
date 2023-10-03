namespace Content.Shared.White.ShittyInteraction.Interactions;

[ImplicitDataDefinitionForInheritors]
public abstract class BaseInteractionEvent : CancellableEntityEventArgs
{
    public EntityUid Performer;
    public EntityUid Target;
}

