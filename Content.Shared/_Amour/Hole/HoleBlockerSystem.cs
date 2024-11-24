using Content.Shared.Inventory.Events;

namespace Content.Shared._Amour.Hole;

public sealed class HoleBlockerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HoleBlockerComponent, GotEquippedEvent>(OnHoleBlockerEquipped);
        SubscribeLocalEvent<HoleBlockerComponent, GotUnequippedEvent>(OnHoleBlockerUnequipped);
    }

    private void OnHoleBlockerEquipped(Entity<HoleBlockerComponent> ent, ref GotEquippedEvent args)
    {
        ent.Comp.Equipee = GetNetEntity(args.Equipee);
    }

    private void OnHoleBlockerUnequipped(Entity<HoleBlockerComponent> ent, ref GotUnequippedEvent args)
    {
        ent.Comp.Equipee = NetEntity.Invalid;
    }
}
