using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared._Amour.Hole;

namespace Content.Server._Amour.Hole;

public sealed partial class HoleSystem
{
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;

    public void InitializeSolution()
    {
        SubscribeLocalEvent<HoleSolutionComponent,ComponentInit>(OnSolutionInit);
        SubscribeLocalEvent<HoleSolutionComponent,EntityUnpausedEvent>(OnUnpaused);
    }

    private void OnSolutionInit(EntityUid uid, HoleSolutionComponent component, ComponentInit args)
    {
        component.NextGenerationTime = _gameTiming.CurTime;
        component.Solution = _solutionContainer.EnsureSolution(uid, HoleSolutionComponent.SlotName);
        component.Solution.MaxVolume = component.SubstanceHoldCount;
    }

    private void OnUnpaused(EntityUid uid, HoleSolutionComponent component, EntityUnpausedEvent args)
    {
        component.NextGenerationTime += args.PausedTime;
    }

    private void UpdateSolution(float frameTime)
    {
        return;
        var query = EntityQueryEnumerator<HoleSolutionComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if(_gameTiming.CurTime < component.NextGenerationTime)
                continue;

            component.NextGenerationTime = _gameTiming.CurTime;

            if(component.Solution.Volume >= component.SubstanceHoldCount)
                continue;

            component.Solution.AddReagent(component.SubstanceName,component.SubstanceGenerationCount);
        }
    }
}
