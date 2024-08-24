using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared._White.Chaplain;
using Content.Shared.Ghost;
using Content.Shared.Inventory;

namespace Content.Server._White.Chaplain;

public sealed class ArmamentsBeaconSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArmamentsBeaconComponent, ArmorSelectedEvent>(OnArmorSelected);
    }

    private void OnArmorSelected(Entity<ArmamentsBeaconComponent> ent, ref ArmorSelectedEvent args)
    {
        var entity = args.Actor;
        var index = args.SelectedIndex;

        if (index < 0 || index >= ent.Comp.Armor.Count)
            return;

        _inventorySystem.TryUnequip(entity, "outerClothing", true);
        _inventorySystem.SpawnItemInSlot(entity, "outerClothing", ent.Comp.Armor[index], silent: true);

        if (index < ent.Comp.Helmets.Count && ent.Comp.Helmets[index] != null)
        {
            _inventorySystem.TryUnequip(entity, "head", true);
            _inventorySystem.SpawnItemInSlot(entity, "head", ent.Comp.Helmets[index]!.Value, silent: true);
        }

        Del(ent);
    }
}
