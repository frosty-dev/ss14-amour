using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server._White.PandaSocket.Main;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Ban)]
public sealed class BanCommand : LocalizedCommands
{
    [Dependency] private readonly IPlayerLocator _locator = default!;
    [Dependency] private readonly IBanManager _bans = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly PandaWebManager _pandaWeb = default!; // WD
    [Dependency] private readonly IEntityManager _entMan = default!; // WD

    public override string Command => "ban";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var target = args[0];
        var reason = args[1];
        var minutes = 0u;
        var isGlobalBan = false;

        if (!Enum.TryParse(_cfg.GetCVar(CCVars.ServerBanDefaultSeverity), out NoteSeverity severity))
        {
            _logManager.GetSawmill("admin.server_ban")
                .Warning("Server ban severity could not be parsed from config! Defaulting to high.");
            severity = NoteSeverity.High;
        }

        switch (args.Length)
        {
            case 2:
                break;
            case 3:
                if (!uint.TryParse(args[2], out minutes))
                {
                    shell.WriteLine(Loc.GetString("cmd-ban-invalid-minutes", ("minutes", args[2])));
                    shell.WriteLine(Help);
                    return;
                }

                break;
            case 4:

                if (!uint.TryParse(args[2], out minutes))
                {
                    shell.WriteLine(Loc.GetString("cmd-ban-invalid-minutes", ("minutes", args[2])));
                    shell.WriteLine(Help);
                    return;
                }

                if (!Enum.TryParse(args[3], ignoreCase: true, out severity))
                {
                    shell.WriteLine(Loc.GetString("cmd-ban-invalid-severity", ("severity", args[3])));
                    shell.WriteLine(Help);
                    return;
                }

                break;
            case 5:
                if (!uint.TryParse(args[2], out minutes))
                {
                    shell.WriteLine(Loc.GetString("cmd-ban-invalid-minutes", ("minutes", args[2])));
                    shell.WriteLine(Help);
                    return;
                }

                if (!Enum.TryParse(args[3], ignoreCase: true, out severity))
                {
                    shell.WriteLine(Loc.GetString("cmd-ban-invalid-severity", ("severity", args[3])));
                    shell.WriteLine(Help);
                    return;
                }

                if (!bool.TryParse(args[4], out isGlobalBan))
                {
                    shell.WriteLine(Loc.GetString($"{args[4]} should be True or False.\n{Help}"));
                    shell.WriteLine(Help);
                    return;
                }

                break;
            default:
                shell.WriteLine(Loc.GetString("cmd-ban-invalid-arguments"));
                shell.WriteLine(Help);
                return;
        }

        var located = await _locator.LookupIdByNameOrIdAsync(target);
        var player = shell.Player;

        if (located == null)
        {
            shell.WriteError(Loc.GetString("cmd-ban-player"));
            return;
        }

        var targetUid = located.UserId;
        var targetHWid = located.LastHWId;

        _bans.CreateServerBan(targetUid, target, player?.UserId, null, targetHWid, minutes, severity, reason, isGlobalBan);

        //WD start
        var dbMan = IoCManager.Resolve<IServerDbManager>();
        var banlist = await dbMan.GetServerBansAsync(null, targetUid, null);
        var banId = banlist.Count == 0 ? null : banlist[^1].Id;

        var utkaBanned = new UtkaBannedEvent()
        {
            Ckey = target,
            ACkey = player?.Name,
            Bantype = "server",
            Duration = minutes,
            Global = isGlobalBan,
            Reason = reason,
            Rid = EntitySystem.Get<GameTicker>().RoundId,
            BanId = banId
        };
        _pandaWeb.SendBotPostMessage(utkaBanned);
        _entMan.EventBus.RaiseEvent(EventSource.Local, utkaBanned);
        //WD end
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {

        var durations = new CompletionOption[]
        {
            new("0", LocalizationManager.GetString("cmd-ban-hint-duration-1")),
            new("1440", LocalizationManager.GetString("cmd-ban-hint-duration-2")),
            new("4320", LocalizationManager.GetString("cmd-ban-hint-duration-3")),
            new("10080", LocalizationManager.GetString("cmd-ban-hint-duration-4")),
            new("20160", LocalizationManager.GetString("cmd-ban-hint-duration-5")),
            new("43800", LocalizationManager.GetString("cmd-ban-hint-duration-6")),
        };

        var severities = new CompletionOption[]
        {
            new("none", Loc.GetString("admin-note-editor-severity-none")),
            new("minor", Loc.GetString("admin-note-editor-severity-low")),
            new("medium", Loc.GetString("admin-note-editor-severity-medium")),
            new("high", Loc.GetString("admin-note-editor-severity-high")),
        };

        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),
                LocalizationManager.GetString("cmd-ban-hint")),
            2 => CompletionResult.FromHint(LocalizationManager.GetString("cmd-ban-hint-reason")),
            3 => CompletionResult.FromHintOptions(durations, LocalizationManager.GetString("cmd-ban-hint-duration")),
            4 => CompletionResult.FromHintOptions(severities, Loc.GetString("cmd-ban-hint-severity")),
            _ => args.Length == 5 ? CompletionResult.FromHint("<server>") : CompletionResult.Empty
        };
    }
}
