using Content.Shared._Amour.ProtocolCRAB17;
using Content.Server.GameTicking;
using Content.Server.Popups;

namespace Content.Server._Amour.ProtocolCRAB17;

public sealed class ProtocolCRAB17System : SharedProtocolCRAB17System
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly ProtocolCRAB17Rule _protocolCRAB17Rule = default!;

    public override void OnDoAfter(Entity<ProtocolCRAB17Component> ent, ref ProtocolCRAB17DoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (_gameTicker.IsGameRuleActive("ProtocolCRAB17Event"))
        {
            _popupSystem.PopupEntity(Loc.GetString("protocol-CRAB17-event-running"), ent, args.User);
            args.Handled = true;
            return;
        }

        if (!_protocolCRAB17Rule.CanStartEvent())
        {
            _popupSystem.PopupEntity(Loc.GetString("protocol-CRAB17-timeout"), ent, args.User);
            args.Handled = true;
            return;
        }

        _popupSystem.PopupEntity(Loc.GetString("protocol-CRAB17-activated"), ent, args.User);
        _gameTicker.AddGameRule("ProtocolCRAB17Event");
        args.Handled = true;
        return;
    }

}
