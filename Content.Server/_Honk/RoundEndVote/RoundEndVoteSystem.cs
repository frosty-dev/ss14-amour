using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Server.Voting.Managers;
using Content.Shared.Voting;


namespace Content.Server._Honk.RoundEndVote;


public sealed class RoundEndVoteSystem : EntitySystem
{
    [Dependency] private readonly IVoteManager _voteManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundEndSystemChangedEvent>(OnRoundEndSystemChange);
    }

    private void OnRoundEndSystemChange(RoundEndSystemChangedEvent ev)
    {
        //Эта строка проверяет, не находится ли игра в состоянии "PreRoundLobby".
        if(_gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
            return;

        _voteManager.CreateStandardVote(null, StandardVoteType.Preset);
        _voteManager.CreateStandardVote(null, StandardVoteType.Map);
    }

}
