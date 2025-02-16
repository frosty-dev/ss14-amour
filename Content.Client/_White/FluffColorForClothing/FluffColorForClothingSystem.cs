using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Shared._White.FluffColorForClothing;

public sealed class FluffColorForClothingSystem : SharedFluffColorForClothingSystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;
    [Dependency] private readonly SharedItemSystem _itemSystem = default!;

    protected override void UpdateVisuals(EntityUid uid, FluffColorForClothingComponent component)
    {
        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        var state = sprite.LayerGetState(0).Name;
        if (state == null)
            return;

        var prefix = state.Substring(0, state.IndexOf('_'));
        sprite.LayerSetState(0, $"{prefix}_{component.CurrentColor}");

        if (TryComp<ClothingComponent>(uid, out var clothingComp))
            _clothingSystem.SetEquippedPrefix(uid, component.CurrentColor, clothingComp);

        if (TryComp<ItemComponent>(uid, out var itemComp))
            _itemSystem.SetHeldPrefix(uid, component.CurrentColor, false, itemComp);
    }

}
