using System.Linq;
using Content.Server.Administration;
using Content.Server.Objectives.Conditions;
using Content.Server.Players;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.White.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class SetCustomObjectiveProgress : IConsoleCommand
{
    public string Command => "setobjprogress";
    public string Description => "Устонавливает прогресс на кастомную цель.";
    public string Help => "setobjprogress <username> <index> <progress>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteLine("Expected exactly 3 arguments: setobjprogress <username> <index> <progress>");
            return;
        }

        var mgr = IoCManager.Resolve<IPlayerManager>();
        if (!mgr.TryGetPlayerDataByUsername(args[0], out var data))
        {
            shell.WriteLine("Can't find the playerdata.");
            return;
        }

        var mind = data.ContentData()?.Mind;
        if (mind == null)
        {
            shell.WriteLine("Can't find the mind.");
            return;
        }

        if (!int.TryParse(args[1], out var i))
        {
            shell.WriteLine($"Invalid index {args[1]}!");
            return;
        }

        if (i < 0 || i >= mind.Objectives.Count)
        {
            shell.WriteLine("Неверный индекс.");
            return;
        }

        var objective = mind.Objectives[i];
        var condition = objective.Conditions.OfType<CustomCondition>().FirstOrDefault();

        if (condition == null)
        {
            shell.WriteLine("Выбранная цель не является кастомной.");
            return;
        }

        if (!float.TryParse(args[2], out var progress) || progress is < 0 or > 1)
        {
            shell.WriteLine($"{args[1]} не является числом от 0 до 1.");
            return;
        }

        condition.CustomProgress = progress;
    }
}
