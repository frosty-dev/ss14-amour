using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Administration;

public abstract class SharedHoursPanelSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<HoursPanelMessage>(OnHoursPanelMessage);
    }

    protected virtual void OnHoursPanelMessage(HoursPanelMessage message, EntitySessionEventArgs eventArgs)
    {
    }
}
