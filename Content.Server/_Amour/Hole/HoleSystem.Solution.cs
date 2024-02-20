using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared._Amour.Arousal;
using Content.Shared._Amour.Hole;
using Content.Shared.Chemistry.Components;

namespace Content.Server._Amour.Hole;

public sealed partial class HoleSystem
{
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;

    public void InitializeSolution()
    {
        SubscribeLocalEvent<HoleSolutionComponent,ComponentInit>(OnSolutionInit);
        SubscribeLocalEvent<HoleSolutionComponent,EntityUnpausedEvent>(OnUnpaused);
        SubscribeLocalEvent<HoleSolutionComponent,ArousalSplash>(OnSplash);
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

    public void SplitSolution(Entity<HoleSolutionComponent?> entity, Entity<SolutionComponent?>? to = null)
    {
        if(!Resolve(entity,ref entity.Comp))
            return;

        if (!to.HasValue)
        {
            entity.Comp.Solution.SplitSolution(entity.Comp.Solution.Volume);
        }
        else
        {
            var entityUid = to.Value;
            if(Resolve(entityUid, ref entityUid.Comp))
            {
                _solutionContainer.AddSolution(
                    new Entity<SolutionComponent>(entityUid.Owner,entityUid.Comp)
                    , entity.Comp.Solution);
            }
        }
    }

    private void OnSplash(EntityUid uid, HoleSolutionComponent component, ref ArousalSplash args)
    {
        if(!TryComp<HoleContainerComponent>(uid,out var holeContainer))
            return;

        var holeUid = GetEntity(holeContainer.MainHole);

        if(!TryComp<HoleComponent>(holeUid,out var holeComponent))
            return;

        SplitSolution(holeUid.Value);
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
