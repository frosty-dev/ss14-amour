using Content.Server.Administration;
using Content.Shared._White;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server._White.Commands;

[AdminCommand(AdminFlags.Round)]
sealed class MapVotingCommand : IConsoleCommand
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    public string Command => "mapvoting";
    public string Description => "Переключить установку карты между голосованием (true) и случайным выбором (false).";
    public string Help => "mapvoting <bool>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !bool.TryParse(args[0], out var value))
        {
            shell.WriteError($"{args[0]} is not a valid boolean.");
            return;
        }

        if (value)
        {
            _configurationManager.SetCVar(WhiteCVars.MapVotingEnabled, true);
            shell.WriteLine("Включена ротация карт методом голосования.");
        }
        else
        {
            _configurationManager.SetCVar(WhiteCVars.MapVotingEnabled, false);
            shell.WriteLine("Включена ротация карт случайным образом.");
        }

    }
}
