using Content.Server.GameTicking;
using Content.Shared.GameTicking;

namespace Content.Server._White.EndOfRoundStats.Command;

public sealed class CommandStatSystem : EntitySystem
{
    public List<(string, string)> eorStats = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        foreach (var (stat, color) in eorStats)
        {
            ev.AddLine($"[color={color}]{stat}[/color]");
        }
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        eorStats.Clear();
    }
}
