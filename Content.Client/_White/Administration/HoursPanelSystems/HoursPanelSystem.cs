using Content.Shared._White.Administration;
using static Content.Client._White.Administration.HoursPanelSystems.HoursPanel;

namespace Content.Client._White.Administration.HoursPanelSystems;

public sealed class HoursPanelSystem : EntitySystem
{
    public HoursPanel? _panel;
  
  
 

    //private HoursPanel _panel;
    public HoursPanelSystem()
    {

    }


    public override void Initialize()
    {
        base.Initialize();
     
        SubscribeNetworkEvent<HoursPanelMessageToClient>(OnHoursPanelMessage);
    }

    private void OnHoursPanelMessage(HoursPanelMessageToClient message, EntitySessionEventArgs eventArgs)
    {
        _panel?.UpdateTime(message.Time);
    }

    public void SendPlayerTimeRequest(HoursPanelMessageToServer message)
    {
        var _entityManager = IoCManager.Resolve<EntityManager>();
        var _che = _entityManager.System<CheZaHuetaSystem>();
        _che.SendNetMessage(message);
      //  RaiseNetworkEvent(message);
    }
}
