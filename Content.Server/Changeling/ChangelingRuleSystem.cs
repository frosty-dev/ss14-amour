using System.Linq;
using Content.Server._Miracle.GulagSystem;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Shared._White.Mood;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.Changeling;

public sealed class ChangelingRuleSystem : GameRuleSystem<ChangelingRuleComponent>
{
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangelingRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected);

        SubscribeLocalEvent<ChangelingRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
    }
    private void AfterEntitySelected(Entity<ChangelingRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        MakeChangeling(args.EntityUid, ent);
    }

    private void OnObjectivesTextGetInfo(
        EntityUid uid,
        ChangelingRuleComponent comp,
        ref ObjectivesTextGetInfoEvent args)
    {
        args.Minds = comp.ChangelingMinds.Select(mindId => (mindId, Comp<MindComponent>(mindId).CharacterName ?? "?")).ToList();
        args.AgentName = Loc.GetString("changeling-round-end-agent-name");
    }

    public void MakeChangeling(EntityUid changeling, ChangelingRuleComponent rule, bool giveObjectives = true)
    {
        if (!_mindSystem.TryGetMind(changeling, out var mindId, out var mind))
            return;

        rule.ChangelingMinds.Add(mindId);

        if (!giveObjectives)
            return;

        var difficulty = 0f;
        for (var pick = 0; pick < rule.ChangelingMaxPicks && rule.ChangelingMaxDifficulty > difficulty; pick++)
        {
            var objective = _objectives.GetRandomObjective(mindId, mind, "ChangelingObjectiveGroups");
            if (objective == null)
                continue;

            _mindSystem.AddObjective(mindId, mind, objective.Value);
            var adding = Comp<ObjectiveComponent>(objective.Value).Difficulty;
            difficulty += adding;
            Log.Debug($"Added objective {ToPrettyString(objective):objective} with {adding} difficulty");
        }
    }

}
