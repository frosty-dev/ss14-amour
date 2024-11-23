using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.PDA.Ringer;
using Content.Server.Roles;
using Content.Server.Traitor.Uplink;
using Content.Shared.FixedPoint;
using Content.Shared.Mind;
using Content.Shared.NPC.Systems;
using Content.Shared.Objectives.Components;
using Content.Shared.PDA;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using System.Text;
using Content.Server.GameTicking.Components;
using Content.Server._White.AspectsSystem.Aspects;
using Content.Server._White.AspectsSystem.Aspects.Components;
using Content.Server.Chat.Managers;
using Content.Shared._White.Mood;

namespace Content.Server.GameTicking.Rules;

public sealed class TraitorRuleSystem : GameRuleSystem<TraitorRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly UplinkSystem _uplink = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;
    //WD EDIT
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public const int MaxPicks = 20;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TraitorRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected);

        SubscribeLocalEvent<TraitorRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
        SubscribeLocalEvent<TraitorRuleComponent, ObjectivesTextPrependEvent>(OnObjectivesTextPrepend);
    }

    protected override void Added(EntityUid uid, TraitorRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);
        MakeCodewords(component);
    }

    private void AfterEntitySelected(Entity<TraitorRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        MakeTraitor(args.EntityUid, ent);
    }

    private void MakeCodewords(TraitorRuleComponent component)
    {
        var adjectives = _prototypeManager.Index(component.CodewordAdjectives).Values;
        var verbs = _prototypeManager.Index(component.CodewordVerbs).Values;
        var codewordPool = adjectives.Concat(verbs).ToList();
        var finalCodewordCount = Math.Min(component.CodewordCount, codewordPool.Count);
        component.Codewords = new string[finalCodewordCount];
        for (var i = 0; i < finalCodewordCount; i++)
        {
            component.Codewords[i] = _random.PickAndTake(codewordPool);
        }
    }

    public bool MakeTraitor(EntityUid traitor, TraitorRuleComponent component, bool giveObjectives = true) // WD EDIT AHEAD OF WIZDEN UPSTREAM
    {
        //Grab the mind if it wasnt provided
        if (!_mindSystem.TryGetMind(traitor, out var mindId, out var mind))
            return false;

        // WD START
        var richAspect = _gameTicker.GetActiveGameRules().Where(HasComp<TraitorRichAspectComponent>).Any();
        // WD END
        // WD EDIT START AHEAD OF WIZDEN UPSTREAM
        var briefing = "";

        if (component.GiveCodewords)
            briefing = Loc.GetString("traitor-role-codewords-short", ("codewords", string.Join(", ", component.Codewords)));
        // WD EDIT END AHEAD OF WIZDEN UPSTREAM
        var issuer = _random.Pick(_prototypeManager.Index(component.ObjectiveIssuers).Values);

        Note[]? code = null;
        if (component.GiveUplink) // WD EDIT AHEAD OF WIZDEN UPSTREAM
        {
            // Calculate the amount of currency on the uplink.
            var startingBalance = component.StartingBalance;
            if (_jobs.MindTryGetJob(mindId, out _, out var prototype))
            // WD EDIT START START AHEAD OF WIZDEN UPSTREAM
            {
                if (startingBalance < prototype.AntagAdvantage) // Can't use Math functions on FixedPoint2
                    startingBalance = 0;
                else
                    startingBalance = startingBalance - prototype.AntagAdvantage;
            }

            if (richAspect)
            {
                startingBalance += 10;
            }

            // Choose and generate an Uplink, and return the uplink code if applicable
            var uplinkParams = RequestUplink(traitor, startingBalance, briefing);
            code = uplinkParams.Item1;
            briefing = uplinkParams.Item2;
        }

        string[]? codewords = null;
        if (component.GiveCodewords)
            codewords = component.Codewords;

        if (component.GiveBriefing)
            _antag.SendBriefing(traitor, GenerateBriefing(codewords, code, issuer), null, component.GreetSoundNotification);
        // WD EDIT END AHEAD OF WIZDEN UPSTREAM
        component.TraitorMinds.Add(mindId);

        // Assign briefing
        _roleSystem.MindAddRole(mindId, new RoleBriefingComponent
        {
            Briefing = briefing
        }, mind, true);

        if (richAspect) // WD
            TraitorRichAspect.NotifyTraitor(mind, _chatManager);

        // Give traitors their objectives
        if (giveObjectives)
        {
            var difficulty = 0f;
            for (var pick = 0; pick < MaxPicks && component.MaxDifficulty > difficulty; pick++)
            {
                var objective = _objectives.GetRandomObjective(mindId, mind, component.ObjectiveGroup);
                if (objective == null)
                    continue;

                _mindSystem.AddObjective(mindId, mind, objective.Value);
                var adding = Comp<ObjectiveComponent>(objective.Value).Difficulty;
                difficulty += adding;
                Log.Debug($"Added objective {ToPrettyString(objective):objective} with {adding} difficulty");
            }
        }

        return true;
    }
    // WD EDIT START AHEAD OF WIZDEN UPSTREAM
    private (Note[]?, string) RequestUplink(EntityUid traitor, FixedPoint2 startingBalance, string briefing)
    {
        var pda = _uplink.FindUplinkTarget(traitor);
        Note[]? code = null;

        var uplinked = _uplink.AddUplink(traitor, startingBalance, pda, true);

        if (pda is not null && uplinked)
        {
            // Codes are only generated if the uplink is a PDA
            code = EnsureComp<RingerUplinkComponent>(pda.Value).Code;

            // If giveUplink is false the uplink code part is omitted
            briefing = string.Format("{0}\n{1}",
                briefing,
                Loc.GetString("traitor-role-uplink-code-short", ("code", string.Join("-", code).Replace("sharp", "#"))));
            return (code, briefing);
        }
        else if (pda is null && uplinked)
        {
            briefing += "\n" + Loc.GetString("traitor-role-uplink-implant-short");
        }

        return (null, briefing);
    }
    // WD EDIT END AHEAD OF WIZDEN UPSTREAM
    private void OnObjectivesTextGetInfo(EntityUid uid, TraitorRuleComponent comp, ref ObjectivesTextGetInfoEvent args)
    {
        args.Minds = _antag.GetAntagMindEntityUids(uid);
        args.AgentName = Loc.GetString("traitor-round-end-agent-name");
    }

    private void OnObjectivesTextPrepend(EntityUid uid, TraitorRuleComponent comp, ref ObjectivesTextPrependEvent args)
    {
        args.Text += "\n" + Loc.GetString("traitor-round-end-codewords", ("codewords", string.Join(", ", comp.Codewords)));
    }

    private string GenerateBriefing(string[]? codewords, Note[]? uplinkCode, string? objectiveIssuer = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString("traitor-role-greeting", ("corporation", objectiveIssuer ?? Loc.GetString("objective-issuer-unknown"))));
        if (codewords != null) // WD EDIT AHEAD OF WIZDEN UPSTREAM
            sb.AppendLine(Loc.GetString("traitor-role-codewords", ("codewords", string.Join(", ", codewords))));
        if (uplinkCode != null)
            sb.AppendLine(Loc.GetString("traitor-role-uplink-code-short", ("code", string.Join("-", uplinkCode).Replace("sharp", "#"))));
        else // WD EDIT AHEAD OF WIZDEN UPSTREAM
            sb.AppendLine(Loc.GetString("traitor-role-uplink-implant"));

        return sb.ToString();
    }

    public List<(EntityUid Id, MindComponent Mind)> GetOtherTraitorMindsAliveAndConnected(MindComponent? ourMind)
    {
        List<(EntityUid Id, MindComponent Mind)> allTraitors = new();

        var query = EntityQueryEnumerator<TraitorRuleComponent>();
        while (query.MoveNext(out var uid, out var traitor))
        {
            foreach (var role in GetOtherTraitorMindsAliveAndConnected(ourMind, (uid, traitor)))
            {
                if (!allTraitors.Contains(role))
                    allTraitors.Add(role);
            }
        }

        return allTraitors;
    }

    private List<(EntityUid Id, MindComponent Mind)> GetOtherTraitorMindsAliveAndConnected(MindComponent? ourMind, Entity<TraitorRuleComponent> rule)
    {
        var traitors = new List<(EntityUid Id, MindComponent Mind)>();
        foreach (var mind in _antag.GetAntagMinds(rule.Owner))
        {
            if (mind.Comp == ourMind)
                continue;

            traitors.Add((mind, mind));
        }

        return traitors;
    }
}
