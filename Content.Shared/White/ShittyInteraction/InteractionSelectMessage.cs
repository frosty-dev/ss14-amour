using Robust.Shared.Serialization;

namespace Content.Shared.White.ShittyInteraction;

[Serializable, NetSerializable]
public sealed class InteractionSelectedMessage : BoundUserInterfaceMessage
{
    public string SelectedInteraction;

    public InteractionSelectedMessage(string selectedInteraction)
    {
        SelectedInteraction = selectedInteraction;
    }
}

[Serializable, NetSerializable]
public sealed class InteractionAvailableState : BoundUserInterfaceState
{
    public List<string> AvailableInteractions;

    public InteractionAvailableState(List<string> availableInteractions)
    {
        AvailableInteractions = availableInteractions;
    }
}


[Serializable, NetSerializable]
public sealed class InteractionAvailableMessage : BoundUserInterfaceMessage
{
    public List<string> AvailableInteractions;

    public InteractionAvailableMessage(List<string> availableInteractions)
    {
        AvailableInteractions = availableInteractions;
    }
}
