using Content.Server.Voting.Managers;
using Content.Shared.GameTicking;
using Content.Shared.Voting;

namespace Content.Server.White.Golosovanie;

public sealed class GolosovanieSystem : EntitySystem
{
    [Dependency] private readonly IVoteManager _vote = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundRestartCleanupEvent ev)
    {
        Logger.Debug("MEOW! END NAHUI");
        _vote.CreateStandardVote(null,StandardVoteType.Map);
        _vote.CreateStandardVote(null,StandardVoteType.Preset);
    }
}
