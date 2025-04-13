using Content.Shared.Interaction.Events;
using Content.Shared._White.Cult;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Player;
using CultistComponent = Content.Shared._White.Cult.Components.CultistComponent;

namespace Content.Server._White.Cult.Structures;

public sealed class CultStructureCraftSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RunicMetalComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, RunicMetalComponent component, UseInHandEvent args)
    {
        if (!HasComp<CultistComponent>(args.User))
            return;

        if(_uiSystem.HasUi(args.User, component.UserInterfaceKey))
            _uiSystem.CloseUi(uid, component.UserInterfaceKey, args.User);

        _uiSystem.OpenUi(uid, component.UserInterfaceKey, args.User);
    }
}
