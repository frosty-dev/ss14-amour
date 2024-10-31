using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Verbs;
using Robust.Shared.Timing;

namespace Content.Server._White.AutoRegenReagent
{
    /// <summary>
    /// So we have a solution name in AutoRegenReagent comp. We will try to get it and start generating reagents. When we switch reagents we clear the solution and start generating different reagent.
    /// </summary>
    public sealed class AutoRegenReagentSystem : EntitySystem
    {
        [Dependency] private readonly SolutionContainerSystem _solutionSystem = default!;
        [Dependency] private readonly PopupSystem _popups = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AutoRegenReagentComponent, ComponentInit>(OnCompInit);
            SubscribeLocalEvent<AutoRegenReagentComponent, GetVerbsEvent<AlternativeVerb>>(AddSwitchVerb);
            SubscribeLocalEvent<AutoRegenReagentComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<AutoRegenReagentComponent, UseInHandEvent>(OnUseInHand,
                before: new[] { typeof(ChemistrySystem) });
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            var query = EntityQueryEnumerator<AutoRegenReagentComponent>();
            while (query.MoveNext(out var uid, out var autoComp))
            {
                if (_timing.CurTime < autoComp.NextUpdate)
                    return;

                autoComp.NextUpdate += autoComp.Interval;

                if (autoComp.Solution == null)
                    TryGetSolution(uid, autoComp);

                if (autoComp.Solution == null)
                    return;

                _solutionSystem.TryAddReagent(autoComp.Solution.Value, autoComp.CurrentReagent, autoComp.UnitsPerInterval);
            }
        }

        private void OnCompInit(EntityUid uid, AutoRegenReagentComponent component, ComponentInit args)
        {
            component.NextUpdate = _timing.CurTime + component.Interval;
            SwitchReagent(component);
        }

        private void OnUseInHand(EntityUid uid, AutoRegenReagentComponent component, UseInHandEvent args)
        {
            if (args.Handled)
                return;

            if (component.Reagents.Count <= 1)
                return;

            SwitchReagent(component, args.User);
            args.Handled = true;
        }

        private void OnExamined(EntityUid uid, AutoRegenReagentComponent component, ExaminedEvent args)
        {
            args.PushMarkup(Loc.GetString("reagent-name", ("reagent", component.CurrentReagent)));
        }

        private void AddSwitchVerb(EntityUid uid, AutoRegenReagentComponent component,
            GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (component.Reagents.Count <= 1)
                return;

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    SwitchReagent(component, args.User);
                },
                Text = Loc.GetString("autoreagent-switch"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void TryGetSolution(EntityUid uid, AutoRegenReagentComponent component)
        {
            if (component.SolutionName == null)
                return;

            if (!_solutionSystem.TryGetSolution(uid, component.SolutionName, out var solution))
                return;

            component.Solution = solution;

            component.CurrentReagent = component.Reagents[component.CurrentIndex];
        }

        private string SwitchReagent(AutoRegenReagentComponent component, EntityUid? user = null)
        {
            if (component.CurrentIndex + 1 == component.Reagents.Count)
                component.CurrentIndex = 0;
            else
                component.CurrentIndex++;

            if (component.Solution != null)
                _solutionSystem.RemoveAllSolution(component.Solution.Value);

            component.CurrentReagent = component.Reagents[component.CurrentIndex];

            if (user != null)
                _popups.PopupEntity(Loc.GetString("autoregen-switched", ("reagent", component.CurrentReagent)), user.Value, user.Value);

            return component.CurrentReagent;
        }
    }
}
