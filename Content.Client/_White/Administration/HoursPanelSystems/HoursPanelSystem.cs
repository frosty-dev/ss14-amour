using Content.Shared._White.Administration;

namespace Content.Client._White.Administration.HoursPanelSystems;

public sealed class HoursPanelSystem : EntitySystem
{
    public HoursPanel? Panel;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<HoursPanelMessageToClient>(OnHoursPanelMessage);
    }

    private void OnHoursPanelMessage(HoursPanelMessageToClient message, EntitySessionEventArgs eventArgs)
    {
        Panel?.UpdateTime(message.Time);
    }

    public void SendPlayerTimeRequest(HoursPanelMessageToServer message)
    {
        RaiseNetworkEvent(message);
    }
}
