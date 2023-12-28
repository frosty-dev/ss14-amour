using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Electrocution;
using Robust.Server.Containers;

namespace Content.Server.White.ShockClothing;

public sealed class ShockClothingSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShockClothingComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShockClothingComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnInit(EntityUid uid, ShockClothingComponent component, ComponentInit args)
    {
        _deviceLink.EnsureSinkPorts(uid, component.TriggerPort);
    }

    private void OnSignalReceived(EntityUid uid, ShockClothingComponent component, ref SignalReceivedEvent args)
    {
        if (args.Port != component.TriggerPort)
            return;

        _audio.PlayPvs(component.ZapSound, uid);

        if (!_containerSystem.TryGetContainingContainer(uid, out var container))
            return;

        _electrocution.TryDoElectrocution(container.Owner, uid, null, TimeSpan.FromSeconds(5), true,
            ignoreInsulation: true);
    }
}
