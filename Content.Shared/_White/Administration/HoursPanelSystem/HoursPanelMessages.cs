using Robust.Shared.Serialization;

namespace Content.Shared._White.Administration;

[Serializable, NetSerializable]
public sealed class HoursPanelMessageToServer : EntityEventArgs
{
    public string PlayerCKey { get; }
    public string Job { get; }
    public HoursPanelMessageToServer(string playerCKey, string job)
    {
        PlayerCKey = playerCKey;
        Job = job;
    }
}

[Serializable, NetSerializable]
public sealed class HoursPanelMessageToClient : EntityEventArgs
{
    public TimeSpan Time { get; }
    public HoursPanelMessageToClient(TimeSpan time)
    {
        Time = time;
    }
}
