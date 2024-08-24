using Content.Server.GameTicking.Rules;
using Content.Server.Antag;
using Content.Server.Mind;
using Content.Server.RoundEnd;
using Content.Shared.Mobs;
using System.Linq;
using Content.Server.Objectives;
using Content.Server.StationEvents.Components;
using Content.Shared._White.Mood;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server._White.Wizard;

/// <summary>
/// This handles...
/// </summary>
public sealed class WizardRuleSystem : GameRuleSystem<WizardRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected);

        SubscribeLocalEvent<WizardComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<WizardRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
    }

    private void OnObjectivesTextGetInfo(Entity<WizardRuleComponent> ent, ref ObjectivesTextGetInfoEvent args)
    {
        args.Minds = ent.Comp.WizardMinds;
        args.AgentName = Loc.GetString("wizard-round-end-agent-name");
    }

    private void OnMobStateChanged(EntityUid uid, WizardComponent component, MobStateChangedEvent ev)
    {
        if (ev.NewMobState == MobState.Dead && component.EndRoundOnDeath)
            CheckAnnouncement();
    }

    private void GiveObjectives(EntityUid mindId, MindComponent mind, WizardRuleComponent wizardRule)
    {
        _mind.TryAddObjective(mindId, mind, "WizardSurviveObjective");

        var difficulty = 0f;
        for (var pick = 0; pick < 6 && 8 > difficulty; pick++)
        {
            var objective = _objectives.GetRandomObjective(mindId, mind, wizardRule.ObjectiveGroup);
            if (objective == null)
                continue;

            _mind.AddObjective(mindId, mind, objective.Value);
            var adding = Comp<ObjectiveComponent>(objective.Value).Difficulty;
            difficulty += adding;
        }
    }

    private void AfterEntitySelected(Entity<WizardRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        MakeWizard(args.EntityUid, ent);
    }

    private void MakeWizard(EntityUid wizard, WizardRuleComponent component, bool giveObjectives = true)
    {
        if (!_mind.TryGetMind(wizard, out var mindId, out var mind))
            return;

        component.WizardMinds.Add(mindId);

        if (!giveObjectives)
            return;

        GiveObjectives(mindId, mind, component);
    }

    private void CheckAnnouncement()
    {
        // Check for all at once gamemode
        if (GameTicker.GetActiveGameRules().Where(HasComp<RampingStationEventSchedulerComponent>).Any())
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var wizard, out _))
        {
            _roundEndSystem.DoRoundEndBehavior(
                wizard.RoundEndBehavior, wizard.EvacShuttleTime, wizard.RoundEndTextSender,
                wizard.RoundEndTextShuttleCall, wizard.RoundEndTextAnnouncement);

            return;
        }
    }

}
