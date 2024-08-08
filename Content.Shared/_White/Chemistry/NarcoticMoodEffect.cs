using Content.Shared._White.Mood;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Chemistry;

[UsedImplicitly]
public sealed partial class NarcoticMoodEffect : ReagentEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return null;
    }

    public override void Effect(ReagentEffectArgs args)
    {
        args.EntityManager.EventBus.RaiseLocalEvent(args.SolutionEntity, new MoodEffectEvent("Stimulator"));
    }
}
