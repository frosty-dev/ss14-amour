using Content.Shared.CombatMode.Pacification;
using Content.Server.Bible.Components;

namespace Content.Server._White.Cult;
public sealed class HellSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HellComponent, ComponentStartup>(PentagramAdded);
    }

    private void PentagramAdded(EntityUid uid, HellComponent component, ComponentStartup args)
    {
        EnsureComp<PacifiedComponent>(uid);
        EnsureComp<BibleUserComponent>(uid);
    }

}
