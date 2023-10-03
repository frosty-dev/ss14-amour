using Robust.Shared.Serialization;

namespace Content.Shared.White.ShittyInteraction.Interactions;

[ImplicitDataDefinitionForInheritors]
[Serializable,NetSerializable]
public abstract class BaseInteractionEvent : CancellableEntityEventArgs
{
    public EntityUid Performer;
    public EntityUid Target;
}

