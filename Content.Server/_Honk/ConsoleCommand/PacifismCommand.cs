using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.CombatMode.Pacification;
using Robust.Shared.Console;

namespace Content.Server._Honk.ConsoleCommand;

[AdminCommand(AdminFlags.Admin)]
public sealed class PacifismCommand : IConsoleCommand
{
    [Dependency] private readonly EntityManager _entityManager = default!;

    public string Command => "addpacifism";
    public string Description => "Add Pacifism Component";
    public string Help => "Poxui mne";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;

        if (player!.AttachedEntity == null)
        {
            shell.WriteLine("You don't have an entity to add a pacified to.");
            return;
        }

        var entity = player.AttachedEntity.Value;
        shell.WriteLine($"Adding PacifiedComponent to {entity}");
        _entityManager.AddComponent<PacifiedComponent>(entity);
    }
}
