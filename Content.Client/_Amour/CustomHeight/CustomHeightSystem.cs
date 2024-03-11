using System.Numerics;
using Content.Shared._Amour.CustomHeight;
using Robust.Client.GameObjects;

namespace Content.Client._Amour.CustomHeight;

public sealed class CustomHeightSystem : SharedCustomHeightSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomHeightComponent,AppearanceChangeEvent>(OnHeightChange);
    }

    private void OnHeightChange(EntityUid uid, CustomHeightComponent component, AppearanceChangeEvent args)
    {
        if(args.Sprite is null || !AppearanceSystem.TryGetData<float>(uid, HeightVisuals.State, out var height))
            return;

        height = Math.Clamp(height, component.Min, component.Max);

        args.Sprite.Scale = new Vector2(height);
    }
}
