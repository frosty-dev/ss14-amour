using Content.Shared._White.Administration;
using static Content.Client._White.Administration.HoursPanelSystems.HoursPanel;

namespace Content.Client._White.Administration.HoursPanelSystems;

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
