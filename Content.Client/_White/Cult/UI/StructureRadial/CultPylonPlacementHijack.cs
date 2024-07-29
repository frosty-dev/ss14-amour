using System.Linq;
using Content.Client.Construction;
using Content.Shared._White.Cult.Pylon;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Popups;
using Robust.Client.Placement;
using Robust.Client.Utility;
using Robust.Shared.Map;

namespace Content.Client._White.Cult.UI.StructureRadial;

public sealed class CultPylonPlacementHijack(ConstructionPrototype? prototype, IEntityManager entMan, EntityUid player)
    : PlacementHijack
{
    private readonly ConstructionSystem _constructionSystem = entMan.System<ConstructionSystem>();

    public override bool CanRotate { get; } = prototype?.CanRotate ?? true;

    /// <inheritdoc />
    public override bool HijackPlacementRequest(EntityCoordinates coordinates)
    {
        if (prototype == null)
            return true;

        if (SharedPylonComponent.CheckForStructure(coordinates, entMan, 10f))
        {
            var popup = entMan.System<SharedPopupSystem>();
            popup.PopupClient(Loc.GetString("cult-structure-craft-another-structure-nearby"), player, player);
            return true;
        }

        _constructionSystem.ClearAllGhosts();
        var dir = Manager.Direction;
        _constructionSystem.SpawnGhost(prototype, coordinates, dir);

        return true;
    }

    /// <inheritdoc />
    public override bool HijackDeletion(EntityUid entity)
    {
        if (IoCManager.Resolve<IEntityManager>().HasComponent<ConstructionGhostComponent>(entity))
        {
            _constructionSystem.ClearGhost(entity.GetHashCode());
        }

        return true;
    }

    /// <inheritdoc />
    public override void StartHijack(PlacementManager manager)
    {
        base.StartHijack(manager);
        manager.CurrentTextures = prototype?.Layers.Select(sprite => sprite.DirFrame0()).ToList();
    }
}
