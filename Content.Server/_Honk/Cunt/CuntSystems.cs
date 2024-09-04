using Content.Server.Chat.Systems;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Shared._Honk.Cunt;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Server._Honk.Cunt;

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
        _solutionContainer.SplitSolution(uid,cuntSolution,50);

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

            var generated = new Solution("Cunt", 2);

            _solutionContainer.TryAddSolution(uid, solution, generated);
        }
    }
    //public override void Update(float frameTime)
    //{
    //    base.Update(frameTime);
//
    //    var query = EntityQueryEnumerator<CuntableComponent, SolutionContainerManagerComponent>();
     //   while (query.MoveNext(out var uid, out var regen, out var manager))
     //   {
     //       if (_timing.CurTime < regen.NextRegenTime)
     //           continue;

     //       regen.NextRegenTime = _timing.CurTime + regen.Duration;
     //       if (_solutionContainer.TryGetSolution(uid, CuntableComponent.CuntSolutionName, out var solution, manager))
     //       {
     //           if (solution.AvailableVolume <= FixedPoint2.Zero)
     //               continue;
//
     //           var generated = new Solution("Cunt", 5);
//
      //          _solutionContainer.TryAddSolution(uid, solution, generated);
      //      }
      //  }
    //}

    private void OnUnpaused(EntityUid uid, CuntableComponent comp, ref EntityUnpausedEvent args)
    {
        comp.NextRegenTime += args.PausedTime;
    }
}
