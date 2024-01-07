using System.Linq;
using Content.Server.Administration;
using Content.Server.Objectives;
using Content.Server.Objectives.Conditions;
using Content.Server.Players;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.White.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class AddCustomObjectiveCommand : IConsoleCommand
{
    public string Command => "addcustomobjective";
    public string Description => "Добавляет кастомную цель игроку.";
    public string Help => "addcustomobjective <username> <title> <description>";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteLine("Expected exactly 3 arguments: addcustomobjective <username> <title> <description>");
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

        if (!IoCManager.Resolve<IPrototypeManager>()
                .TryIndex<ObjectivePrototype>("CustomObjective", out var objectivePrototype))
        {
            shell.WriteLine("Can't find matching prototype.");
            return;
        }

        var objective = objectivePrototype.GetObjective(mind);
        if (mind.Objectives.Contains(objective))
            return;

        foreach (var condition in objective.Conditions.OfType<CustomCondition>())
        {
            condition.CustomTitle = args[1];
            condition.CustomDesc = args[2];
        }

        mind.Objectives.Add(objective);
    }
}
