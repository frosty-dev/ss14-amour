using Content.Server.Popups;
using Content.Shared._White.Cult.Pylon;
using Content.Shared.Construction;
using Content.Shared.Popups;
using JetBrains.Annotations;

namespace Content.Server._White.Cult.Pylon;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CheckForStructure : IGraphAction
{
    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        var xform = entityManager.GetComponent<TransformComponent>(uid);
        var coords = xform.Coordinates;
        if (!SharedPylonComponent.CheckForStructure(coords, entityManager, 9f, uid))
            return;

        entityManager.QueueDeleteEntity(uid);
        if (userUid != null)
        {
            entityManager.System<PopupSystem>()
                .PopupEntity(Loc.GetString("cult-structure-craft-another-structure-nearby"),
                    userUid.Value, userUid.Value, PopupType.MediumCaution);
        }
        entityManager.SpawnEntity("CultRunicMetal4", coords);
    }
}
