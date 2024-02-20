using Content.Server.EUI;
using Content.Shared._Amour.Hole;
using Content.Shared.Verbs;
using Robust.Server.Player;

namespace Content.Server._Amour.InteractionPanel;

public sealed class InteractionPanelSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<HoleContainerComponent,GetVerbsEvent<Verb>>(OnVerb);
    }

    private void OnVerb(EntityUid uid, HoleContainerComponent component, GetVerbsEvent<Verb> args)
    {
        if(!_playerManager.TryGetSessionByEntity(uid, out var session))
            return;
        args.Verbs.Add(new Verb()
        {
            Text = "MEOW",
            Act = () => _eui.OpenEui(new InteractionPanelEui(), session)
        });
    }
}
