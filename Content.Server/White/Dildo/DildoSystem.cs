using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.White.Dildo;
using Robust.Shared.Timing;

namespace Content.Server.White.Dildo;

public sealed class DildoSystem : SharedDildoSystem
{
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DildoComponent,UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<DildoComponent,ComponentInit>(OnInit);
        SubscribeLocalEvent<DildoComponent,SignalReceivedEvent>(OnsignalReceived);
    }

    private void OnsignalReceived(EntityUid uid, DildoComponent component,ref SignalReceivedEvent args)
    {
        if(args.Port != component.TogglePort)
            return;

        ToggleVibration(uid,component);
    }

    private void OnInit(EntityUid uid, DildoComponent component, ComponentInit args)
    {
        _signalSystem.EnsureSinkPorts(uid,component.TogglePort);
    }

    private void OnUseInHand(EntityUid uid, DildoComponent component, UseInHandEvent args)
    {
        ToggleVibration(uid,component);
    }
}
