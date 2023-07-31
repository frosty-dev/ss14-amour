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
    }

    private void OnStartup(EntityUid uid, CuntableComponent component, ComponentStartup args)
    {
        var cuntSolution = _solutionContainer.EnsureSolution(uid, CuntableComponent.CuntSolutionName);
        cuntSolution.MaxVolume = 10;
    }

    private void OnCunt(EntityUid uid, CuntableComponent component, ref EmoteEvent args)
    {
        if (args.Emote.ID == "Squelch")
        {
            GenCum(uid, component);
            TryCunt(uid, component);
        }
    }

    public bool TryCunt(EntityUid uid, CuntableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if(!_solutionContainer.TryGetSolution(uid,CuntableComponent.CuntSolutionName,out var cuntSolution))
            return false;

        if(cuntSolution.Volume < 5)
            return false;

        _puddle.TrySpillAt(uid, cuntSolution, out _);
        _solutionContainer.SplitSolution(uid,cuntSolution,cuntSolution.MaxVolume);

        return true;
    }

    public void GenCum(EntityUid uid, CuntableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return ;

        if (_solutionContainer.TryGetSolution(uid, CuntableComponent.CuntSolutionName, out var solution))
        {
            if (solution.AvailableVolume <= FixedPoint2.Zero)
               return;

            var generated = new Solution("Cunt", 1);

            _solutionContainer.TryAddSolution(uid, solution, generated);
        }
    }
}
