using Content.Shared._White.Administration;
using Robust.Server.Player;
using Content.Server.Players.PlayTimeTracking;

namespace Content.Server._White.Administration;

public sealed class HoursPanelSystem : SharedHoursPanelSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPlayTimeTrackingManager _playTimeTracking = default!;
    protected override void OnHoursPanelMessage(HoursPanelMessage message, EntitySessionEventArgs eventArgs)
    {
        if (message.Time != null)
            return;

        if (_playerManager.TryGetSessionByUsername(message.PlayerCKey, out var player))
            return;

        if (message.Job == "Overall")
        {
            var timer = _playTimeTracking.GetOverallPlaytime(player!);
            RaiseNetworkEvent(new HoursPanelMessage(message.PlayerCKey, message.Job, timer));
            return;
        }
    }
}
