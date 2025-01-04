using Content.Shared._White.Administration;

namespace Content.Client._White.Administration;

public sealed class HoursPanelSystem : SharedHoursPanelSystem
{
    public HoursPanel Panel { get; }
    public HoursPanelSystem(HoursPanel panel)
    {
        Panel = panel;
    }

    protected override void OnHoursPanelMessage(HoursPanelMessage message, EntitySessionEventArgs eventArgs)
    {
        if (message.Time != null)
            Panel.UpdateTime(message.Time);
    }

    public void SendPlayerTimeRequest(HoursPanelMessage message)
    {
        RaiseNetworkEvent(message);
    }
}
