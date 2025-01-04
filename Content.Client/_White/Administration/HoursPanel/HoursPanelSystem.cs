using Content.Shared._White.Administration;

namespace Content.Client._White.Administration;

public sealed class HoursPanelSystem : EntitySystem
{
    private HoursPanel _panel;
    public HoursPanelSystem(HoursPanel panel)
    {
        _panel = panel;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<HoursPanelMessageToClient>(OnHoursPanelMessage);
    }

    private void OnHoursPanelMessage(HoursPanelMessageToClient message, EntitySessionEventArgs eventArgs)
    {
        _panel.UpdateTime(message.Time);
    }

    public void SendPlayerTimeRequest(HoursPanelMessageToServer message)
    {
        RaiseNetworkEvent(message);
    }
}
