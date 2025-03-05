using Robust.Shared.Serialization;

namespace Content.Shared._White.DeepSpaceCom;

[Serializable, NetSerializable]
public enum DeepSpaceComUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class DeepSpaceComBoundUIState : BoundUserInterfaceState
{
    public bool MicEnabled;
    public bool SpeakerEnabled;
    public List<string> AvailableChannels;
    public string SelectedChannel;

    public DeepSpaceComBoundUIState(bool micEnabled, bool speakerEnabled, List<string> availableChannels, string selectedChannel)
    {
        MicEnabled = micEnabled;
        SpeakerEnabled = speakerEnabled;
        AvailableChannels = availableChannels;
        SelectedChannel = selectedChannel;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleDeepSpaceComMicrophoneMessage : BoundUserInterfaceMessage
{
    public bool Enabled;

    public ToggleDeepSpaceComMicrophoneMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleDeepSpaceComSpeakerMessage : BoundUserInterfaceMessage
{
    public bool Enabled;

    public ToggleDeepSpaceComSpeakerMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

[Serializable, NetSerializable]
public sealed class SelectDeepSpaceComChannelMessage : BoundUserInterfaceMessage
{
    public string Channel;

    public SelectDeepSpaceComChannelMessage(string channel)
    {
        Channel = channel;
    }
}
