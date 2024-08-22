using Content.Client.Weapons.Ranged.Components;
using Content.Shared._White.WeaponModules;
using Content.Shared.Rounding;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._White.WeaponsModules;

public sealed partial class WeaponModulesVisuals : VisualizerSystem<WeaponModulesComponent>
{
[Dependency] private readonly PointLightSystem _lightSystem = default!;
    protected override void OnAppearanceChange(EntityUid uid, WeaponModulesComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if(args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<string>(uid, ModuleVisualState.BarrelModule, out var barrelModule, args.Component))
        {
            if (barrelModule.Length != 0 && barrelModule != "none")
            {
                args.Sprite.LayerSetState(ModuleVisualState.BarrelModule, barrelModule);
                args.Sprite.LayerSetVisible(ModuleVisualState.BarrelModule, true);
            }
            else
                args.Sprite.LayerSetVisible(ModuleVisualState.BarrelModule, false);
        }

        if (AppearanceSystem.TryGetData<string>(uid, ModuleVisualState.HandGuardModule, out var handguardModule, args.Component))
        {
            if (handguardModule.Length != 0 && handguardModule != "none")
            {
                args.Sprite.LayerSetState(ModuleVisualState.HandGuardModule, handguardModule);
                args.Sprite.LayerSetVisible(ModuleVisualState.HandGuardModule, true);
            }
            else
                args.Sprite.LayerSetVisible(ModuleVisualState.HandGuardModule, false);
        }

        if (AppearanceSystem.TryGetData<string>(uid, ModuleVisualState.AimModule, out var aimModule, args.Component))
        {
            if (aimModule.Length != 0 && aimModule != "none")
            {
                args.Sprite.LayerSetState(ModuleVisualState.AimModule, aimModule);
                args.Sprite.LayerSetVisible(ModuleVisualState.AimModule, true);
            }
            else
                args.Sprite.LayerSetVisible(ModuleVisualState.AimModule, false);
        }

        if (AppearanceSystem.TryGetData<string>(uid, ModuleVisualState.ShutterModule, out var shutterModule, args.Component))
        {
            if (shutterModule.Length != 0 && shutterModule != "none")
            {
                args.Sprite.LayerSetState(ModuleVisualState.ShutterModule, shutterModule);
                args.Sprite.LayerSetVisible(ModuleVisualState.ShutterModule, true);
            }
            else
                args.Sprite.LayerSetVisible(ModuleVisualState.ShutterModule, false);
        }

        if (AppearanceSystem.TryGetData(uid, Modules.Light, out _, args.Component))
        {
            if (TryComp<PointLightComponent>(uid, out var pointLightComponent))
            {
                if(!pointLightComponent.Enabled)
                    return;

                _lightSystem.SetMask("/Textures/White/Effects/LightMasks/lightModule.png", pointLightComponent!);
            }
        }
    }
}
