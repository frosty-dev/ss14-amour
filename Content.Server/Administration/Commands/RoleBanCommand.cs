﻿using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Ban)]
public sealed class RoleBanCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerLocator _locator = default!;
    [Dependency] private readonly IBanManager _bans = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public string Command => "roleban";
    public string Description => Loc.GetString("cmd-roleban-desc");
    public string Help => Loc.GetString("cmd-roleban-help");

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var target = args[0];
        var job = args[1];
        var reason = args[2];
        var minutes = 0u;
        var isGlobalBan = false;
        if (!Enum.TryParse(_cfg.GetCVar(CCVars.RoleBanDefaultSeverity), out NoteSeverity severity))
        {
            Logger.WarningS("admin.role_ban", "Role ban severity could not be parsed from config! Defaulting to medium.");
            severity = NoteSeverity.Medium;
        }

        switch (args.Length)
        {
            case 3:
                break;
            case 4:
                if (!uint.TryParse(args[3], out minutes))
                {
                    shell.WriteError(Loc.GetString("cmd-roleban-minutes-parse", ("time", args[3]), ("help", Help)));
                    return;
                }

                break;
            case 5:
                if (!uint.TryParse(args[3], out minutes))
                {
                    shell.WriteError(Loc.GetString("cmd-roleban-minutes-parse", ("time", args[3]), ("help", Help)));
                    return;
                }
                if (!Enum.TryParse(args[4], ignoreCase: true, out severity))
                {
                    shell.WriteLine(Loc.GetString("cmd-roleban-severity-parse", ("severity", args[4]), ("help", Help)));
                    return;
                }
                break;
            case 6:
                if (!uint.TryParse(args[3], out minutes))
                {
                    shell.WriteError(Loc.GetString("cmd-roleban-minutes-parse", ("time", args[3]), ("help", Help)));
                    return;
                }
                if (!Enum.TryParse(args[4], ignoreCase: true, out severity))
                {
                    shell.WriteLine(Loc.GetString("cmd-roleban-severity-parse", ("severity", args[4]), ("help", Help)));
                    return;
                }
                if (!bool.TryParse(args[5], out isGlobalBan))
                {
                    shell.WriteLine($"{args[5]} should be True or False.\n{Help}");
                    return;
                }
                break;
            default:
                shell.WriteError(Loc.GetString("cmd-roleban-arg-count"));
                shell.WriteLine(Help);
                return;
        }

        var located = await _locator.LookupIdByNameOrIdAsync(target);
        if (located == null)
        {
            shell.WriteError(Loc.GetString("cmd-roleban-name-parse"));
            return;
        }

        var targetUid = located.UserId;
        var targetHWid = located.LastHWId;
        _bans.CreateRoleBan(targetUid, located.Username, shell.Player?.UserId, null, targetHWid, job, minutes, severity, reason, DateTimeOffset.UtcNow, isGlobalBan);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var durOpts = new CompletionOption[]
        {
            new("0", Loc.GetString("cmd-roleban-hint-duration-1")),
            new("1440", Loc.GetString("cmd-roleban-hint-duration-2")),
            new("4320", Loc.GetString("cmd-roleban-hint-duration-3")),
            new("10080", Loc.GetString("cmd-roleban-hint-duration-4")),
            new("20160", Loc.GetString("cmd-roleban-hint-duration-5")),
            new("43800", Loc.GetString("cmd-roleban-hint-duration-6")),
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
                Loc.GetString("cmd-roleban-hint-1")),
            2 => CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<JobPrototype>(),
                Loc.GetString("cmd-roleban-hint-2")),
            3 => CompletionResult.FromHint(Loc.GetString("cmd-roleban-hint-3")),
            4 => CompletionResult.FromHintOptions(durOpts, Loc.GetString("cmd-roleban-hint-4")),
            5 => CompletionResult.FromHintOptions(severities, Loc.GetString("cmd-roleban-hint-5")),
            6 => CompletionResult.FromHint("<server>"),
            _ => CompletionResult.Empty
        };
    }
}
