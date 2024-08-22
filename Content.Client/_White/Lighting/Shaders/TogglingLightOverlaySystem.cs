using Content.Shared._White.Lighting.Shaders;
using Content.Shared.Power;
using Robust.Client.GameObjects;

namespace Content.Client._White.Lighting.Shaders;

public sealed class TogglingLightOverlaySystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<LightingOverlayComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, LightingOverlayComponent component, AppearanceChangeEvent args)
    {
        if (!args.AppearanceData.TryGetValue(PowerDeviceVisuals.Powered, out var state))
            return;

        component.Enabled = (bool) state;
    }
}
