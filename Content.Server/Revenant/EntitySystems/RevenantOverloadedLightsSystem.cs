using Content.Server.Lightning;
using Content.Shared.Revenant.Components;
using Content.Shared.Revenant.EntitySystems;

namespace Content.Server.Revenant.EntitySystems;

/// <summary>
/// This handles...
/// </summary>
public sealed class RevenantOverloadedLightsSystem : SharedRevenantOverloadedLightsSystem
{
    [Dependency] private readonly LightningSystem _lightning = default!; // WD EDIT

    protected override void OnZap(Entity<RevenantOverloadedLightsComponent> lights)
    {
        var component = lights.Comp;
        if (component.Target == null)
            return;

        var lxform = Transform(lights);
        var txform = Transform(component.Target.Value);

        if (!lxform.Coordinates.TryDistance(EntityManager, txform.Coordinates, out var distance))
            return;
        if (distance > component.ZapRange)
            return;

        _lightning.ShootLightning(lights, component.Target.Value, component.ZapBeamEntityId, false); // WD EDIT
    }
}
