
using Content.Shared.Examine;
using Content.Shared.Implants;

namespace Content.Server._White.AddImplant;

public sealed class AddImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedSubdermalImplantSystem _implantSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AddImplantComponent, MapInitEvent>(OnMapInit);
    }
    public void OnMapInit(Entity<AddImplantComponent> ent, ref MapInitEvent args)
    {
        _implantSystem.AddImplants(ent.Owner, ent.Comp.Implants);
        RemComp<AddImplantComponent>(ent.Owner);
    }
}
