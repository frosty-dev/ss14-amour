using Content.Server.Voting.Managers;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Voting;
using Content.Shared.White;
using Robust.Shared.Configuration;

namespace Content.Server.White.Golosovanie;

public sealed class GolosovanieSystem : EntitySystem
{
    [Dependency] private readonly IVoteManager _vote = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundRestartCleanupEvent ev)
    {
        if (!_configurationManager.GetCVar(WhiteCVars.AutoVote))
            return;

        _configurationManager.SetCVar(CCVars.GameMap, string.Empty);
        _vote.CreateStandardVote(null,StandardVoteType.Map);
        _vote.CreateStandardVote(null,StandardVoteType.Preset);
    }
}
