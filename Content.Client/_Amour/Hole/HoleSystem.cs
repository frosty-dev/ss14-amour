using Content.Shared._Amour.Hole;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Reflection;
using Content.Shared.Foldable;

namespace Content.Client._Amour.Hole;

public sealed class HoleSystem : SharedHoleSystem
{
    [Dependency] private readonly IReflectionManager _reflection = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HoleContainerComponent, EntInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<HoleContainerComponent, EntRemovedFromContainerMessage>(OnRemoved);

        SubscribeLocalEvent<HoleComponent, ComponentHandleState>(OnHandleState);

        SubscribeLocalEvent<HoleBlockerComponent, FoldedEvent>(OnFolded);
    }

    private void OnFolded(Entity<HoleBlockerComponent> ent, ref FoldedEvent args)
    {
        if (ent.Comp.Equipee == NetEntity.Invalid)
            return;
        var equipee = GetEntity(ent.Comp.Equipee);
        if (!HasComp<HoleContainerComponent>(equipee))
            return;
        UpdateVisuals(equipee);
    }

    private void OnHandleState(EntityUid uid, HoleComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not HoleComponentState componentState)
            return;

        component.IsExcited = componentState.IsExcited;
        component.Parent = componentState.Parent;

        if (component.Parent is not null)
        {
            UpdateVisual(GetEntity(component.Parent.Value), uid, !HasAccessTo(GetEntity(component.Parent.Value), uid));
        }
    }

    private void OnRemoved(EntityUid uid, HoleContainerComponent component, EntRemovedFromContainerMessage args)
    {
        if (!HasComp<HumanoidAppearanceComponent>(uid))
            return;

        UpdateVisuals(uid);
        if (args.Container != component.Slot)
            return;

        UpdateVisual(uid, args.Entity, true);
    }

    private void OnInsert(EntityUid uid, HoleContainerComponent component, EntInsertedIntoContainerMessage args)
    {
        if (!HasComp<HumanoidAppearanceComponent>(uid))
            return;

        UpdateVisuals(uid);
    }

    private void UpdateVisuals(Entity<HoleContainerComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp) || entity.Comp.Slot == null)
            return;

        foreach (var hole in entity.Comp.Slot.ContainedEntities)
        {
            UpdateVisual(entity.Owner, hole, !HasAccessTo(entity, hole));
        }
    }

    private void UpdateVisual(Entity<SpriteComponent?, HumanoidAppearanceComponent?, HoleContainerComponent?> owner, Entity<HoleComponent?> entity, bool clear = false)
    {
        if (!Resolve(owner.Owner, ref owner.Comp1, ref owner.Comp3) || !Resolve(entity.Owner, ref entity.Comp))
            return;

        var spriteComp = owner.Comp1;
        var holeComp = entity.Comp;

        foreach (var layer in holeComp.Layers)
        {
            if (string.IsNullOrEmpty(layer.RsiPath))
                layer.RsiPath = holeComp.RsiPath;

            if (Resolve(owner.Owner, ref owner.Comp2))
            {
                layer.Color ??= owner.Comp2.SkinColor;
            }

            var state = layer.State;

            if (holeComp.Prefixes.Count == 0)
            {
                if (clear)
                    spriteComp.LayerSetVisible(GenitalVisualLayers.DickFront, false);
                else
                {
                    spriteComp.LayerSetData(GenitalVisualLayers.DickFront, layer);
                    spriteComp.LayerSetVisible(GenitalVisualLayers.DickFront, true);
                }
                return;
            }

            foreach (var prefix in holeComp.Prefixes)
            {
                if (!_reflection.TryParseEnumReference(prefix.Layer, out var @enum))
                {
                    Log.Error("FUCK! ERROR WITH " + prefix.Layer);
                    continue;
                }

                if (clear)
                {
                    spriteComp.LayerSetVisible(@enum, false);
                }
                else
                {
                    var s = "";
                    if (prefix.HasForHuman && owner.Comp3.UseHumanGenitalLayers)
                        s = "_s";

                    var mainPrefix = prefix.Prefix;
                    if (holeComp.IsExcited && !string.IsNullOrEmpty(prefix.ExcitedPrefix))
                        mainPrefix = prefix.ExcitedPrefix;

                    layer.State = state + s + mainPrefix;
                    spriteComp.LayerSetData(@enum, layer);
                    spriteComp.LayerSetVisible(@enum, true);
                    layer.State = state;
                }

            }
        }
    }

    public override void Exide(Entity<HoleComponent?> entity, bool value = true)
    {
        if (!Resolve(entity.Owner, ref entity.Comp)) return;
        base.Exide(entity, value);
        var netEntity = entity.Comp.Parent;
        if (netEntity != null)
            UpdateVisual(GetEntity(netEntity.Value), entity);
    }
}
