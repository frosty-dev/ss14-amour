using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Server.Voting.Managers;
using Content.Shared.Voting;
using Robust.Server.Player;


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

        var player = _playerManager.PlayerCount >= 20;

        if (player)
        {
            _voteManager.CreateStandardVote(null, StandardVoteType.Preset);
        }
        else
        {
            _gameTicker.SetGamePreset("Secret");
        }

        _voteManager.CreateStandardVote(null, StandardVoteType.Map);
    }

}
