using Content.Shared._Amour.Hole;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;

namespace Content.Client._Amour.Hole;

public sealed class HoleSystem : SharedHoleSystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<HoleContainerComponent,EntInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<HoleContainerComponent,EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnRemoved(EntityUid uid, HoleContainerComponent component, EntRemovedFromContainerMessage args)
    {
        Log.Debug("A VSE");
    }

    private void OnInsert(EntityUid uid, HoleContainerComponent component, EntInsertedIntoContainerMessage args)
    {
        if(!TryComp<SpriteComponent>(uid, out var sprite) || !TryComp<HoleComponent>(args.Entity,out var hole))
            return;
        Log.Debug("DRAW PISYA");

        DrawShit(uid,args.Entity);
    }

    private void DrawShit(Entity<SpriteComponent?,HumanoidAppearanceComponent?> owner, Entity<HoleComponent?> entity)
    {
        if(!Resolve(owner.Owner,ref owner.Comp1, ref owner.Comp2) || !Resolve(entity.Owner,ref entity.Comp))
            return;

        var spriteComp = owner.Comp1;
        var holeComp = entity.Comp;

        foreach (var layer in holeComp.BehindLayer)
        {
            var l = spriteComp.AddLayer(layer, 0);
            spriteComp[l].Color = layer.Color ?? owner.Comp2.SkinColor;
        }

        foreach (var layer in holeComp.FrontLayer)
        {
            var l = spriteComp.AddLayer(layer);
            spriteComp[l].Color = layer.Color ?? owner.Comp2.SkinColor;
        }
    }
}
