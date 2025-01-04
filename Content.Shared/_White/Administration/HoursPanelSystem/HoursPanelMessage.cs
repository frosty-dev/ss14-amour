using Robust.Shared.Serialization;

namespace Content.Shared._White.Administration;

[Serializable, NetSerializable]
public sealed class HoursPanelMessage : EntityEventArgs
{
    public string PlayerCKey { get; }
    public string Job { get; }
    public TimeSpan? Time { get; }
    public HoursPanelMessage(string playerCKey, string job, TimeSpan? time = null)
    {
        PlayerCKey = playerCKey;
        Job = job;
        Time = time;
    }
}
