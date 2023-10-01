using Robust.Shared.Serialization;

namespace Content.Shared.White.Interaction.Interactions
{
    [Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
    public abstract class BaseInteractionEvent : CancellableEntityEventArgs
    {
        public EntityUid Performer;
        public EntityUid Target;
    }
}
