using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._Miracle.Nya;

[AdminCommand(AdminFlags.Admin)]
public sealed class ScreenGrabCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public  string Command => "nyagrab";
    public string Description => "nyagrab!!";
    public string Help  => "nyagrab player";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var sys = _entityManager.EntitySysManager.GetEntitySystem<NyaGrabSystem>();
        if (args.Length < 1)
        {
            var player = shell.Player;
            var toKickPlayer = player ?? _players.Sessions.FirstOrDefault();
            if (toKickPlayer == null)
            {
                shell.WriteLine("You need to provide a player to nyagrab.");
                return;
            }

            shell.WriteLine(
                $"You need to provide a player to nyagrab. Try running 'nyagrab {toKickPlayer.Name}' as an example.");
            return;
        }

        var name = args[0];

        if (_players.TryGetSessionByUsername(name, out var target))
        {
            sys.RequestScreengrab(target);
        }
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class NyaCheckCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "nyacheck";
    public string Description => "nyacheck!!";
    public string Help => "nyacheck player";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var sys = _entityManager.EntitySysManager.GetEntitySystem<CheatCheckSystem>();

        if (args.Length < 1)
        {
            var player = shell.Player;
            var toKickPlayer = player ?? _players.Sessions.FirstOrDefault();
            if (toKickPlayer == null)
            {
                shell.WriteLine("You need to provide a player to nyacheck.");
                return;
            }

            shell.WriteLine(
                $"You need to provide a player to nyacheck. Try running 'nyacheck {toKickPlayer.Name}' as an example.");
            return;
        }

        var name = args[0];

        if (_players.TryGetSessionByUsername(name, out var target))
        {
            sys.RequestCheck(target);
        }
    }
}
