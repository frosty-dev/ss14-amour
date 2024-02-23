using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.InteractionPanel;

public abstract class InteractionBaseEvent : HandledEntityEventArgs
{
    public ProtoId<InteractionPrototype> Id;
    public Entity<InteractionPanelComponent> Performer;
    public Entity<InteractionPanelComponent> Target;

    protected InteractionBaseEvent(ProtoId<InteractionPrototype> id, Entity<InteractionPanelComponent> performer, Entity<InteractionPanelComponent> target)
    {
        Id = id;
        Performer = performer;
        Target = target;
    }
}

public sealed class InteractionBeginningEvent : InteractionBaseEvent
{
    public InteractionBeginningEvent(ProtoId<InteractionPrototype> id, Entity<InteractionPanelComponent> performer, Entity<InteractionPanelComponent> target) : base(id, performer, target)
    {
    }
}

public sealed class InteractionEndingEvent : InteractionBaseEvent
{
    public InteractionEndingEvent(ProtoId<InteractionPrototype> id, Entity<InteractionPanelComponent> performer, Entity<InteractionPanelComponent> target) : base(id, performer, target)
    {
    }
}
