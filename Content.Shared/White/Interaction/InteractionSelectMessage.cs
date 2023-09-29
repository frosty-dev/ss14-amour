using Robust.Shared.Serialization;

namespace Content.Shared.White.Interaction;

[Serializable, NetSerializable]
public sealed class InteractionSelectMessage : EntityEventArgs
{
    public string SelectedInteraction;

    public InteractionSelectMessage(string selectedInteraction)
    {
        SelectedInteraction = selectedInteraction;
    }
}
