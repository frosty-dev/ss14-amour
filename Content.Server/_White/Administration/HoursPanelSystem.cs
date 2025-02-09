using Content.Shared._White.Administration;
using Robust.Server.Player;
using Content.Server.Players.PlayTimeTracking;

namespace Content.Server._White.Administration;

public sealed class HoursPanelSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPlayTimeTrackingManager _playTimeTracking = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<HoursPanelMessageToServer>(OnHoursPanelMessage);
    }

    private void OnHoursPanelMessage(HoursPanelMessageToServer message, EntitySessionEventArgs eventArgs)
    {
        if (!_playerManager.TryGetSessionByUsername(message.PlayerCKey, out var player))
            return;

        if (player == null)
            return;

        TimeSpan timer;

        if (message.Job == "Overall")
        {
            timer = _playTimeTracking.GetOverallPlaytime(player!);
        }
        else
        {
            timer = _playTimeTracking.GetPlayTimeForTracker(player!, message.Job);
        }

        RaiseNetworkEvent(new HoursPanelMessageToClient(timer));
    }
}
