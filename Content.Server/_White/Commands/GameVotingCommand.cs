using Content.Server.Administration;
using Content.Shared._White;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server._White.Commands;

[AdminCommand(AdminFlags.Round)]
sealed class GameVotingCommand : IConsoleCommand
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    public string Command => "gamevoting";
    public string Description => "Переключить установку режима игры между голосованием (true) и случайным выбором (false).";
    public string Help => "gamevoting <bool>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !bool.TryParse(args[0], out var value))
        {
            shell.WriteError($"{args[0]} is not a valid boolean.");
            return;
        }

        if (value)
        {
            _configurationManager.SetCVar(WhiteCVars.GameVotingEnabled, true);
            shell.WriteLine("Включена ротация режимов игры методом голосования.");
        }
        else
        {
            _configurationManager.SetCVar(WhiteCVars.GameVotingEnabled, false);
            shell.WriteLine("Включена ротация режимов игры случайным образом.");
        }

    }
}
