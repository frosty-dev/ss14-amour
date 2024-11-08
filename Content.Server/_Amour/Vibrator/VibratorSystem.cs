using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Shared._Amour.Vibrator;
using Content.Shared.Interaction.Events;

namespace Content.Server._Amour.Vibrator;

public sealed class VibratorSystem : SharedVibratorSystem
{
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VibratorComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<VibratorComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<VibratorComponent, SignalReceivedEvent>(OnsignalReceived);
    }

    private void OnsignalReceived(EntityUid uid, VibratorComponent component, ref SignalReceivedEvent args)
    {
        if (args.Port != component.TogglePort)
            return;

        ToggleVibration(uid, component);
    }

    private void OnInit(EntityUid uid, VibratorComponent component, ComponentInit args)
    {
        _signalSystem.EnsureSinkPorts(uid, component.TogglePort);
    }

    private void OnUseInHand(EntityUid uid, VibratorComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        ToggleVibration(uid, component);
        args.Handled = true;
    }
}
