using System.Linq;
using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Presets;
using Content.Shared.Administration;
using Content.Shared.Database;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Round)]
    sealed class ForcePresetCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;

        public string Command => "forcepreset";
        public string Description => "Forces a specific game preset to start for the current lobby.";
        public string Help => $"Usage: {Command} <preset>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var ticker = _entityManager.System<GameTicker>();
            if (ticker.RunLevel != GameRunLevel.PreRoundLobby)
            {
                shell.WriteLine("This can only be executed while the game is in the pre-round lobby.");
                return;
            }

            if (args.Length != 1)
            {
                shell.WriteLine("Need exactly one argument.");
                return;
            }

            var name = args[0];
            if (!ticker.TryFindGamePreset(name, out var type))
            {
                shell.WriteLine($"No preset exists with name {name}.");
                return;
            }

            ticker.SetGamePreset(type, true);

            _adminLogger.Add(LogType.EventStarted, $"Forced {type.ID} for secret.");


            var player = "Someone";

            if (shell.Player != null)
                player = shell.Player.Name;

            _chatManager.SendAdminAnnouncement($"{player} forced {type.ID} for secret.");
            shell.WriteLine($"forced the game to start with preset {name}.");

            ticker.UpdateInfoText();
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var options = IoCManager.Resolve<IPrototypeManager>()
                    .EnumeratePrototypes<GamePresetPrototype>()
                    .OrderBy(p => p.ID)
                    .Select(p => p.ID);

                return CompletionResult.FromHintOptions(options, "<preset>");
            }

            return CompletionResult.Empty;
        }
    }
}
