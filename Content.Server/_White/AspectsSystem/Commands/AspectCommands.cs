using System.Linq;
using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Server._White.AspectsSystem.Managers;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._White.AspectsSystem.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class ForceAspectCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "forceaspect";
    public string Description => "Принудительно форсит аспект по его ID.";
    public string Help => "forceaspect <aspectId>";

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
            shell.WriteError("Использование: forceaspect <aspectId>");
            return;
        }

        var aspectManager = _entityManager.System<AspectManager>();

        var aspectId = args[0];
        var result = aspectManager.ForceAspect(aspectId);
        shell.WriteLine(result);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length != 1)
            return CompletionResult.Empty;

        var aspectManager = _entityManager.System<AspectManager>();

        var options = aspectManager
            .GetAspectsPrototypesId()
            .Select(p => new CompletionOption(p.Key.ID, p.Value.Name))
            .OrderBy(p => p.Value);

        return CompletionResult.FromHintOptions(options, Loc.GetString("forcemap-command-arg-map"));
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class DeForceAspectCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "deforceaspect";
    public string Description => "Дефорсит принудительно установленный аспект.";
    public string Help => "deforceaspect";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var ticker = _entityManager.System<GameTicker>();

        if (ticker.RunLevel != GameRunLevel.PreRoundLobby)
        {
            shell.WriteLine("This can only be executed while the game is in the pre-round lobby.");
            return;
        }

        var aspectManager = _entityManager.System<AspectManager>();

        var result = aspectManager.DeForceAspect();
        shell.WriteLine(result);
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class GetForcedAspectCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "getforcedaspect";
    public string Description => "Получает информацию о принудительно установленном аспекте.";
    public string Help => "getforcedaspect";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var ticker = _entityManager.System<GameTicker>();

        if (ticker.RunLevel != GameRunLevel.PreRoundLobby)
        {
            shell.WriteLine("This can only be executed while the game is in the pre-round lobby.");
            return;
        }

        var aspectManager = _entityManager.System<AspectManager>();

        var result = aspectManager.GetForcedAspect();
        shell.WriteLine(result);
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class ListAspectsCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "listaspects";
    public string Description => "Список всех доступных аспектов.";
    public string Help => "listaspects";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var aspectManager = _entityManager.System<AspectManager>();

        var aspectIds = aspectManager.GetAllAspectIds();

        if (aspectIds.Count == 0)
        {
            shell.WriteLine("Нет доступных аспектов.");
        }
        else
        {
            shell.WriteLine("Список доступных аспектов:");
            foreach (var aspectId in aspectIds)
            {
                shell.WriteLine(aspectId);
            }
        }
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class RunAspectCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "runaspect";
    public string Description => "Запускает аспект по его ID.";
    public string Help => "runaspect <aspectId>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Использование: runaspect <aspectId>");
            return;
        }

        var aspectId = args[0];

        var aspectManager = _entityManager.System<AspectManager>();

        var result = aspectManager.RunAspect(aspectId);
        shell.WriteLine(result);
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class RunRandomAspectCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "runrandomaspect";
    public string Description => "Запускает случайный аспект.";
    public string Help => "runrandomaspect";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var aspectManager = _entityManager.System<AspectManager>();

        var result = aspectManager.RunRandomAspect();
        shell.WriteLine(result);
    }
}
