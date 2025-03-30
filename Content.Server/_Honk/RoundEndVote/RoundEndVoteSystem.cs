using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Server.Voting.Managers;
using Content.Shared._White;
using Content.Shared.CCVar;
using Content.Shared.Voting;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Server._Honk.RoundEndVote;

public sealed class RoundEndVoteSystem : EntitySystem
{
    [Dependency] private readonly IVoteManager _voteManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

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
                SelectingGame();
                SelectingMap();
            });
        }
        else
        {
            SelectingGame();
            SelectingMap();
        }

    }
    private void SelectingGame()
    {
        if (_playerManager.PlayerCount <= 5)
        {
            _gameTicker.SetGamePreset("Greenshift");
        }

        if (_playerManager.PlayerCount > 5 && _playerManager.PlayerCount < 15)
        {
            _gameTicker.SetGamePreset("Extended");
        }

        if (_playerManager.PlayerCount >= 15)
        {
            if (_cfg.GetCVar(WhiteCVars.GameVotingEnabled))
            {
                _voteManager.CreateStandardVote(null, StandardVoteType.Preset);
            }
            else
            {
                _gameTicker.SetGamePreset("Secret");
            }
        }
    }

    private void SelectingMap()
    {
        if (_cfg.GetCVar(WhiteCVars.MapVotingEnabled))
        {
            _voteManager.CreateStandardVote(null, StandardVoteType.Map);
        }
        else
        {
            _cfg.SetCVar(CCVars.GameMap, string.Empty);
        }

    }

}
