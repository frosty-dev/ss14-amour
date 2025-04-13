using System.Linq;
using Content.Server._Miracle.GulagSystem;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.GameTicking.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Hands.Systems;
using Content.Server.Objectives.Components;
using Content.Server.RoundEnd;
using Content.Server.StationEvents.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Roles;
using Robust.Shared.Player;
using Content.Shared._White.Antag;
using Content.Shared._White.Cult.Components;
using Content.Shared._White.Cult.Systems;
using Content.Shared._White.Mood;
using Content.Shared.Alert;
using Content.Shared.Cloning;
using Content.Shared.Mind;
using Robust.Server.Containers;
using Robust.Shared.Random;

namespace Content.Server._White.Cult.GameRule;

public sealed class CultRuleSystem : GameRuleSystem<CultRuleComponent>
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly BloodSpearSystem _bloodSpear = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly GulagSystem _gulag = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected);

        SubscribeLocalEvent<CultNarsieSummoned>(OnNarsieSummon);

        SubscribeLocalEvent<CultistComponent, ComponentRemove>(OnCultistComponentRemoved);
        SubscribeLocalEvent<CultistComponent, MobStateChangedEvent>(OnCultistsStateChanged);
        SubscribeLocalEvent<CultistComponent, CloningEvent>(OnClone);
    }

    protected override void Added(EntityUid uid, CultRuleComponent rule, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, rule, gameRule, args);
    }

    private void OnClone(Entity<CultistComponent> ent, ref CloningEvent args)
    {
        RemoveObjectiveAndRole(ent);
    }

    private void OnNarsieSummon(CultNarsieSummoned ev)
    {
        var query =
            EntityQueryEnumerator<MobStateComponent, MindContainerComponent, CultistComponent, TransformComponent>();

        List<Entity<MindContainerComponent, TransformComponent>> cultists = new();

        while (query.MoveNext(out var uid, out _, out var mindContainer, out _, out var transform))
        {
            cultists.Add((uid, mindContainer, transform));
        }

        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var cult, out _))
        {
            cult.WinCondition = CultWinCondition.Win;
            _roundEndSystem.EndRound();

            foreach (var ent in cultists)
            {
                if (ent.Comp1.Mind is null)
                    continue;

                var reaper = Spawn(cult.ReaperPrototype, ent.Comp2.Coordinates);
                _mindSystem.TransferTo(ent.Comp1.Mind.Value, reaper);

                _bodySystem.GibBody(ent);
            }

            return;
        }
    }

    public void RemoveObjectiveAndRole(EntityUid uid)
    {
        if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind))
            return;

        var objectives = mind.Objectives.FindAll(HasComp<PickCultTargetComponent>);
        foreach (var obj in objectives)
        {
            _mindSystem.TryRemoveObjective(mindId, mind, mind.Objectives.IndexOf(obj));
        }

        if (_roleSystem.MindHasRole<CultistRoleComponent>(mindId))
            _roleSystem.MindRemoveRole<CultistRoleComponent>(mindId);
    }

    private void OnCultistComponentRemoved(EntityUid uid, CultistComponent component, ComponentRemove args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
        {
            cult.CurrentCultists.Remove(component);
        }

        if (!TerminatingOrDeleted(uid))
        {
            RemoveAllCultistItems(uid);
            RemoveCultistAppearance(uid);
            RaiseLocalEvent(uid, new MoodRemoveEffectEvent("CultFocused"));
            _alertsSystem.ClearAlert(uid, AlertType.BloodSpells);
        }

        _bloodSpear.DetachSpearFromUser((uid, component));

        foreach (var empower in component.SelectedEmpowers)
        {
            _actions.RemoveAction(uid, GetEntity(empower));
        }

        CheckRoundShouldEnd();
    }

    private void OnCultistsStateChanged(EntityUid uid, CultistComponent component, MobStateChangedEvent ev)
    {
        if (ev.NewMobState == MobState.Dead)
        {
            CheckRoundShouldEnd();
        }
    }

    public MindComponent? GetTarget()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
        {
            if (cult.CultTarget == null || !TryComp(cult.CultTarget.Value, out MindComponent? mind))
            {
                continue;
            }

            return mind;
        }

        return null;
    }

    public bool CanSummonNarsie()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
        {
            var cultistsAmount = cult.CurrentCultists.Count;
            var constructsAmount = cult.Constructs.Count;
            var enoughCultists = cultistsAmount + constructsAmount > 10;

            if (!enoughCultists)
            {
                return false;
            }

            var target = GetTarget();
            var targetKilled = target == null || _mindSystem.IsCharacterDeadIc(target);

            return targetKilled;
        }

        return false;
    }

    private void CheckRoundShouldEnd()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var cult, out _))
        {
            var aliveCultists = 0;

            foreach (var cultistComponent in cult.CurrentCultists)
            {
                var owner = cultistComponent.Owner;
                if (!TryComp<MobStateComponent>(owner, out var mobState))
                    continue;

                if (!_mobStateSystem.IsDead(owner, mobState))
                {
                    aliveCultists++;
                }
            }

            if (aliveCultists != 0)
                return;

            cult.WinCondition = CultWinCondition.Failure;

            // Check for all at once gamemode
            if (!GameTicker.GetActiveGameRules().Where(HasComp<RampingStationEventSchedulerComponent>).Any())
                _roundEndSystem.EndRound();
        }
    }

    private void RemoveCultistAppearance(EntityUid cultist)
    {
        if (TryComp<HumanoidAppearanceComponent>(cultist, out var appearanceComponent))
        {
            //Потому что я так сказал
            appearanceComponent.EyeColor = Color.White;
            Dirty(cultist, appearanceComponent);
        }

        RemComp<PentagramComponent>(cultist);
    }

    private void UpdateCultistsAppearance(CultRuleComponent cultRuleComponent)
    {
        var cultistsCount = cultRuleComponent.CurrentCultists.Count;
        var constructsCount = cultRuleComponent.Constructs.Count;
        var totalCultMembers = cultistsCount + constructsCount;
        if (totalCultMembers >= cultRuleComponent.PentagramThreshold)
            cultRuleComponent.Stage = CultStage.Pentagram;
        else if (totalCultMembers >= cultRuleComponent.ReadEyeThreshold && cultRuleComponent.Stage == CultStage.Normal)
            cultRuleComponent.Stage = CultStage.RedEyes;

        if (cultRuleComponent.Stage == CultStage.Normal)
            return;

        foreach (var cultistComponent in cultRuleComponent.CurrentCultists)
        {
            var cultist = cultistComponent.Owner;
            if (TryComp<HumanoidAppearanceComponent>(cultist, out var appearanceComponent))
            {
                appearanceComponent.EyeColor = cultRuleComponent.EyeColor;
                Dirty(cultist, appearanceComponent);
            }

            if (cultRuleComponent.Stage != CultStage.Pentagram)
                return;

            EnsureComp<PentagramComponent>(cultist);
            EnsureComp<GlobalAntagonistComponent>(cultist).AntagonistPrototype = "globalAntagonistCult";
        }
    }

    private void AfterEntitySelected(Entity<CultRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (ent.Comp.CultTarget == null)
        {
            var potentialTargets = FindPotentialTargets();
            if (potentialTargets.Count == 0)
            {
                ent.Comp.CultTarget = null;
                return;
            }
            ent.Comp.CultTarget = _random.PickAndTake(potentialTargets).Mind;
        }

        MakeCultist(args.EntityUid, ent);
    }

    public void MakeCultist(EntityUid cultist, CultRuleComponent rule)
    {
        if (!_mindSystem.TryGetMind(cultist, out var mindId, out var mind))
            return;

        _mindSystem.TryAddObjective(mindId, mind, "KillCultTargetObjective");

        if (!TryComp<CultistComponent>(cultist, out var cultistComponent))
            return;

        rule.CurrentCultists.Add(cultistComponent);
        _alertsSystem.ShowAlert(cultist, AlertType.BloodSpells);

        var name = Name(cultist);

        if (TryComp<ActorComponent>(cultist, out var actor))
        {
            rule.CultistsCache.TryAdd(name, actor.PlayerSession.Name);
            _mindSystem.TryGetMind(actor.PlayerSession.UserId, out var mindEnt);
            cultistComponent.OriginalMind = mindEnt;
        }

        UpdateCultistsAppearance(rule);
    }

    private void RemoveAllCultistItems(EntityUid uid)
    {
        if (!_inventorySystem.TryGetContainerSlotEnumerator(uid, out var enumerator))
            return;

        while (enumerator.MoveNext(out var container))
        {
            if (container.ContainedEntity != null && HasComp<CultItemComponent>(container.ContainedEntity.Value))
            {
                _container.Remove(container.ContainedEntity.Value, container, true, true);
            }
        }

        foreach (var item in _hands.EnumerateHeld(uid))
        {
            if (TryComp(item, out CultItemComponent? cultItem) && !cultItem.CanPickUp &&
                !_hands.TryDrop(uid, item, null, false, false))
                QueueDel(item);
        }
    }

    public void TransferRole(EntityUid transferFrom, EntityUid transferTo)
    {
        if (HasComp<PentagramComponent>(transferFrom))
            EnsureComp<PentagramComponent>(transferTo);

        if (!HasComp<CultistComponent>(transferFrom))
            return;

        var query = EntityQuery<CultRuleComponent>();
        foreach (var cultRule in query)
        {
            cultRule.CultistsCache.Remove(Name(transferFrom));
        }

        EnsureComp<CultistComponent>(transferTo);
        RemComp<CultistComponent>(transferFrom);
    }

    private List<MindContainerComponent> FindPotentialTargets()
    {
        var query = EntityQueryEnumerator<MindContainerComponent, HumanoidAppearanceComponent, ActorComponent>();

        var potentialTargets = new List<MindContainerComponent>();

        while (query.MoveNext(out var uid, out var mind, out _, out var actor))
        {
            var entity = mind.Mind;

            if (entity == default)
                continue;

            if (_gulag.IsUserGulagged(actor.PlayerSession.UserId, out _))
                continue;

            if (HasComp<CultistComponent>(uid))
                continue;

            potentialTargets.Add(mind);
        }

        return potentialTargets;
    }
}
