using System.Linq;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Shared._White.FluffColorForClothing;

public sealed class FluffColorForClothingSystem : SharedFluffColorForClothingSystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private string GetNextColor(FluffColorForClothingComponent component)
    {
        var index = component.Colors.IndexOf(component.CurrentColor);
        var count = component.Colors.Count;
        if (index < count - 1)
            index++;
        else
            index = 0;

        var newColor = component.Colors[index];

        return newColor;
    }

    protected override void ChangeColor(EntityUid uid, FluffColorForClothingComponent component)
    {
        if (component.User != null && _inventory.TryGetContainerSlotEnumerator((EntityUid) component.User, out var containerSlotEnumerator))
        {
            while (containerSlotEnumerator.NextItem(out var item, out var _))
            {
                if (TryComp<FluffColorForClothingComponent>(item, out var comp) && !comp.MainItem)
                {
                    comp.CurrentColor = GetNextColor(comp);
                    Dirty(item, comp);
                }
            }
        }
        ChangeCompInside(component);
        component.CurrentColor = GetNextColor(component);
        Dirty(uid, component);
    }

    private void ChangeCompInside(FluffColorForClothingComponent component)
    {
        if (_container.TryGetContainer(component.Owner, "toggleable-clothing", out var container) && container.ContainedEntities.Any())
        {
            var content = container.ContainedEntities.First();
            if (TryComp<FluffColorForClothingComponent>(content, out var contentComp) && component.Specifier == contentComp.Specifier)
            {
                contentComp.CurrentColor = GetNextColor(contentComp);
                Dirty(contentComp.Owner, contentComp);
            }

        }
    }
}
