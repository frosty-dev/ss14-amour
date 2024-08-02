using Content.Server.Holosign;
using Content.Shared.Destructible;

namespace Content.Server._White.Holo;

public sealed class HoloSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<HoloComponent, DestructionEventArgs>(OnDestruction);
    }

    private void OnDestruction(EntityUid uid, HoloComponent component, DestructionEventArgs args)
    {
        if (!TryComp<HolosignProjectorComponent>(component.Sign, out var holo))
            return;

        holo.Signs.Remove(uid);
    }
}
