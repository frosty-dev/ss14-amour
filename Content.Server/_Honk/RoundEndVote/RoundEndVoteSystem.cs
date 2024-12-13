using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Server.Voting.Managers;
using Content.Shared.Voting;
using Robust.Server.Player;
using Robust.Shared.Timing;

namespace Content.Server._Honk.RoundEndVote;

public sealed class RoundEndVoteSystem : EntitySystem
{
    [Dependency] private readonly IVoteManager _voteManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundEndSystemChangedEvent>(OnRoundEndSystemChange);
    }

    private void OnRoundEndSystemChange(RoundEndSystemChangedEvent ev)
    {
        if (_gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
            return;

        if (_playerManager.PlayerCount == 0)
        {
            _gameTicker.DelayStart(TimeSpan.FromSeconds(60));
            Timer.Spawn(60 * 1000, () =>
            {
                if (_playerManager.PlayerCount >= 15)
                {
                    _voteManager.CreateStandardVote(null, StandardVoteType.Preset);
                }
                else if (_playerManager.PlayerCount >= 5)
                {
                    _gameTicker.SetGamePreset("Extended");
                }
            });
        }

        if (_playerManager.PlayerCount >= 15)
        {
            _voteManager.CreateStandardVote(null, StandardVoteType.Preset);
        }
        else if (_playerManager.PlayerCount >= 5)
        {
            _gameTicker.SetGamePreset("Extended");
        }
        else
        {
            _gameTicker.SetGamePreset("Greenshift");
        }

        _voteManager.CreateStandardVote(null, StandardVoteType.Map);
    }

}
