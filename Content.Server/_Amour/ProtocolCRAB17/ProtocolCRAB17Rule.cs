using Content.Server.Chat.Systems;
using Content.Server.StationEvents.Events;
using Content.Server.GameTicking.Components;
using Content.Shared._Amour.ProtocolCRAB17;
using Robust.Shared.Timing;
using Content.Shared._White.Economy;
using Content.Server._White.Economy;

namespace Content.Server._Amour.ProtocolCRAB17;

public sealed class ProtocolCRAB17Rule : StationEventSystem<ProtocolCRAB17RuleComponent>
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BankCardSystem _bankCardSystem = default!;

    public TimeSpan? LastTimeExecuted;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void Added(EntityUid uid, ProtocolCRAB17RuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (TryGetRandomStation(out var stationUid))
        {
            component.TargetStation = stationUid;
        }

        var query = AllEntityQuery<ProtocolCRAB17Component>();
        while (query.MoveNext(out _, out var comp))
        {
            if (comp.BankAccountId == null)
                continue;

            component.Callers.Add((int) comp.BankAccountId);
        }
    }

    protected override void Started(EntityUid uid, ProtocolCRAB17RuleComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (component.TargetStation == null)
        {
            ForceEndSelf(uid, gameRule);
            return;
        }

        _chatSystem.DispatchStationAnnouncement((EntityUid) component.TargetStation, Loc.GetString("protocol-CRAB17-stage-1"),
            colorOverride: Color.OrangeRed, sender: "Центральное Командование", announcementSound: ProtocolCRAB17RuleComponent.Announcement);
    }

    protected override void Ended(EntityUid uid, ProtocolCRAB17RuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        var allMoneyStolen = 0;

        var query = AllEntityQuery<BankCardComponent>();
        while (query.MoveNext(out _, out var bankCardComponent))
        {
            if (bankCardComponent.AccountId == null)
                continue;

            if (component.Callers.Contains((int) bankCardComponent.AccountId))
                continue;

            var money = _bankCardSystem.GetBalance((int) bankCardComponent.AccountId);
            allMoneyStolen += money;

            _bankCardSystem.TryChangeBalance((int) bankCardComponent.AccountId, -money);
        }

        var callersCount = component.Callers.Count;

        foreach (var caller in component.Callers)
        {
            _bankCardSystem.TryChangeBalance(caller, (int) Math.Floor((double) (allMoneyStolen / callersCount)));
        }

        if (component.TargetStation != null)
            _chatSystem.DispatchStationAnnouncement((EntityUid) component.TargetStation, Loc.GetString("protocol-CRAB17-stage-2", ("amount", allMoneyStolen)),
                colorOverride: Color.OrangeRed, sender: "Центральное Командование");

        var ertsys = _entities.System<ProtocolCRAB17Rule>();
        ertsys.LastTimeExecuted = _timing.CurTime;
        component.Callers.Clear();
    }

    public bool CanStartEvent()
    {
        var ertsys = _entities.System<ProtocolCRAB17Rule>();

        if (ertsys.LastTimeExecuted == null)
            return true;

        else if (ertsys.LastTimeExecuted < _timing.CurTime - ProtocolCRAB17RuleComponent.TimeBetweenEvents)
            return true;

        return false;
    }

}
