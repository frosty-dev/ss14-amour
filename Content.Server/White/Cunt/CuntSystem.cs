using Content.Server.Chat.Systems;
using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Emoting.Components;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Content.Shared.White.Cunt;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.White.Cunt;

public sealed class CuntSystem : EntitySystem
{
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CuntableComponent,EmoteEvent>(OnCunt);
        SubscribeLocalEvent<CuntableComponent,ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CuntableComponent,EntityUnpausedEvent>(OnUnpaused);
    }

    private void OnStartup(EntityUid uid, CuntableComponent component, ComponentStartup args)
    {
        var cuntSolution = _solutionContainer.EnsureSolution(uid, CuntableComponent.CuntSolutionName);
        cuntSolution.MaxVolume = component.CuntCount;
        component.NextRegenTime = _timing.CurTime;
    }

    private void OnUnpaused(EntityUid uid, CuntableComponent comp, ref EntityUnpausedEvent args)
    {
        comp.NextRegenTime += args.PausedTime;
    }

    private void OnCunt(EntityUid uid, CuntableComponent component, ref EmoteEvent args)
    {
        if (args.Emote.ID == "Squelch")
        {
            TryCunt(uid, component);
        }
    }

    public bool TryCunt(EntityUid uid, CuntableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if(!_solutionContainer.TryGetSolution(uid,CuntableComponent.CuntSolutionName,out var cuntSolution))
            return false;

        if(cuntSolution.Volume < component.CuntCount)
            return false;

        _puddle.TrySpillAt(uid, cuntSolution, out _);
        _solutionContainer.SplitSolution(uid,cuntSolution,cuntSolution.MaxVolume);

        return true;
    }

    public void GenCum(EntityUid uid,FixedPoint2 quantity, CuntableComponent? component = null, SolutionContainerManagerComponent? solutionContainerManagerComponent = null)
    {
        if (!Resolve(uid, ref component, ref solutionContainerManagerComponent))
            return ;

        if (_solutionContainer.TryGetSolution(uid, CuntableComponent.CuntSolutionName, out var solution, solutionContainerManagerComponent))
        {
            if (solution.AvailableVolume <= FixedPoint2.Zero)
               return;

            var generated = new Solution("Cunt", quantity);

            _solutionContainer.TryAddSolution(uid, solution, generated);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CuntableComponent, SolutionContainerManagerComponent>();
        while (query.MoveNext(out var uid, out var regen, out var manager))
        {
            if (_timing.CurTime < regen.NextRegenTime)
                continue;

            regen.NextRegenTime = _timing.CurTime + regen.Duration;

            GenCum(uid,regen.CuntGetCount, regen, manager);
        }
    }
}
