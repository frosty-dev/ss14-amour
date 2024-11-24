using Content.Shared.Inventory;
using Content.Shared.Foldable;

namespace Content.Shared._Amour.Hole;

public partial class SharedHoleSystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public bool HasAccessTo(Entity<HoleContainerComponent?, InventoryComponent?> entity, string to)
    {
        if (!Resolve(entity.Owner, ref entity.Comp1) || !TryFind(entity, to, out var hole))
            return false;
        return HasAccessTo(entity, new Entity<HoleComponent?>(hole.Owner, hole.Comp));
    }

    public bool HasAccessTo(Entity<HoleContainerComponent?, InventoryComponent?> entity, Entity<HoleComponent?> hole)
    {
        if (!Resolve(entity, ref entity.Comp1) || !Resolve(hole, ref hole.Comp))
            return false;
        if (!Resolve(entity, ref entity.Comp2))
            return true;

        foreach (var slot in hole.Comp.HoleNotVisibleIn)
        {
            if (slot == "jumpsuit" && _inventory.TryGetSlotEntity(entity, "jumpsuit", out var jumpsuit, entity) && TryComp<FoldableComponent>(jumpsuit, out var foldedItem) && foldedItem.IsFolded && hole.Comp.IsVisibleInFoldedJumpsuit)
                continue;

            if (_inventory.TryGetSlotEntity(entity, slot, out var item, entity) && !(HasComp<VisibleHoleComponent>(item) && hole.Comp.IsVisibleInSkirt))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryFind(Entity<HoleContainerComponent?> entity, string to, out Entity<HoleComponent> hole)
    {
        hole = new Entity<HoleComponent>();
        if (!Resolve(entity.Owner, ref entity.Comp))
            return false;

        foreach (var holeUid in entity.Comp.Slot.ContainedEntities)
        {
            if (!TryComp<HoleComponent>(holeUid, out var holeComponent) || holeComponent.HoleName != to)
                continue;

            hole.Owner = holeUid;
            hole.Comp = holeComponent;
            return true;
        }

        return false;
    }

}
