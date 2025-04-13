using Content.Client._White.Cult.UI.CultistFactory;
using Content.Shared._White.Cult.Components;
using Content.Shared._White.Cult.UI;

namespace Content.Client._White.Cult.UI;

public sealed class CultAltarSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultistFactoryComponent, AfterAutoHandleStateEvent>(OnAltarAfterState);
    }

    private void OnAltarAfterState(Entity<CultistFactoryComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!_uiSystem.TryGetOpenUi<CultistFactoryBUI>(ent.Owner, CultistAltarUiKey.Key, out var bui))
            return;

        bui.Dispose();
    }
}
