using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Server._White.ERTRecruitment;
using Content.Shared.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared._White.AuthPanel;
using Content.Shared._White.GhostRecruitment;
using Content.Shared.Ghost;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Timing;
using Robust.Shared.Player;

namespace Content.Server._White.AuthPanel;

public sealed class AuthPanelSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ERTRecruitmentRule _ert = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    public Dictionary<AuthPanelAction, HashSet<EntityUid>> Counter = new();
    public Dictionary<AuthPanelAction, HashSet<int>> CardIndexes = new();
    public string Reason = "";
    /// <summary>
    /// Minimal Amount of votes needed for action.
    /// </summary>
    public static int MinCount = 3;

    /// <summary>
    /// Amount of minutes before action can be called. Counting from round start.
    /// </summary>
    public static int EarliestStart = 45;

    /// <summary>
    /// Amount of seconds before action can be called. Counting from previous vote.
    /// </summary>
    public static int DelayDuration = 5;

    /// <summary>
    /// Amount of minutes before action can be called. Counting from previous call for vote.
    /// </summary>
    public static int TimeoutDuration = 10;
    private TimeSpan? _delay;
    private TimeSpan? _timeout;
    public override void Initialize()
    {
        SubscribeLocalEvent<AuthPanelComponent, AuthPanelButtonPressedMessage>(OnButtonPressed);
        SubscribeLocalEvent<AuthPanelComponent, AuthPanelPerformActionEvent>(OnPerformAction);
        SubscribeLocalEvent<RecruitedComponent, ERTRecruitedReasonEvent>(OnReason);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
    }

    private void OnReason(EntityUid uid, RecruitedComponent component, ERTRecruitedReasonEvent args)
    {
        args.Reason = Reason;
    }

    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        Counter.Clear();
        CardIndexes.Clear();

        _delay = null;
        _timeout = null;
    }

    private void ClearPanel()
    {
        Counter.Clear();
        CardIndexes.Clear();

        _delay = null;
        _timeout = null;

        var action = new AuthPanelConfirmationAction(AuthPanelAction.ERTRecruit, 0, MinCount, "");
        var query = EntityQueryEnumerator<AuthPanelComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (!_ui.HasUi(uid, AuthPanelUiKey.Key))
                continue;

            var state = new AuthPanelConfirmationActionState(action);
            _ui.SetUiState(uid, AuthPanelUiKey.Key, state);
            _appearance.SetData(uid, AuthPanelVisualLayers.Confirm, false);
        }
    }

    private void OnPerformAction(EntityUid uid, AuthPanelComponent component, AuthPanelPerformActionEvent args)
    {
        if (args.Action is AuthPanelAction.ERTRecruit)
        {

            if (_ticker.RoundDuration() < TimeSpan.FromMinutes(EarliestStart))
            {
                var station = _station.GetStationInMap(Transform(uid).MapID);

                if (station != null)
                    _ert.DeclineERT(station.Value);
                _adminLogger.Add(LogType.EventStarted, LogImpact.High, $"ERT Declined - Not enough time passed");
                return;
            }

            var query = EntityQueryEnumerator<GhostComponent, ActorComponent>();
            var ghostList = new List<EntityUid>();
            while (query.MoveNext(out var ghost, out _, out _))
            {
                ghostList.Add(ghost);
            }

            var playerCount = _playerManager.PlayerCount;
            if (playerCount - ghostList.Count > playerCount / 2 && ghostList.Count > MinCount)
            {
                _gameTicker.AddGameRule(ERTRecruitmentRuleComponent.EventName);
            }
            else
            {
                var station = _station.GetStationInMap(Transform(uid).MapID);

                if (station != null)
                    _ert.DeclineERT(station.Value);
                _adminLogger.Add(LogType.EventStarted, LogImpact.High, $"ERT Declined - Not enough ghosts");
            }

            foreach (var entities in Counter.Values)
            {
                foreach (var entity in entities)
                {
                    _adminLogger.Add(LogType.EventStarted, LogImpact.High,
                        $"{ToPrettyString(entity):player} just called ERT. Reason: {Reason}");
                }
            }
        }
    }

    private void OnButtonPressed(EntityUid uid, AuthPanelComponent component, AuthPanelButtonPressedMessage args)
    {
        var access = _access.FindAccessTags(args.Actor);

        if (!access.Contains("Command"))
        {
            _popup.PopupEntity(Loc.GetString("auth-panel-no-access"),
                args.Actor, args.Actor);
            return;
        }

        if (string.IsNullOrEmpty(args.Reason))
        {
            _popup.PopupEntity(Loc.GetString("auth-panel-no-reason"),
                args.Actor, args.Actor);
            return;
        }

        if (_delay != null)
        {
            _popup.PopupEntity(Loc.GetString("auth-panel-wait"),
                args.Actor, args.Actor);
            return;
        }

        if (!Counter.TryGetValue(args.Button, out var hashSet))
        {
            hashSet = new HashSet<EntityUid>();
            Counter.Add(args.Button, hashSet);
        }

        if (hashSet.Count >= MinCount)
            return;

        if (!CardIndexes.TryGetValue(args.Button, out var cardSet))
        {
            cardSet = new HashSet<int>();
            CardIndexes.Add(args.Button, cardSet);
        }

        if (cardSet.Contains(access.Count))
        {
            _popup.PopupEntity(Loc.GetString("auth-panel-used-ID"),
                args.Actor, args.Actor);
            return;
        }

        if (!hashSet.Add(args.Actor))
        {
            _popup.PopupEntity(Loc.GetString("auth-panel-pressed"),
                args.Actor, args.Actor);
            return;
        }

        cardSet.Add(access.Count);
        _delay = _timing.CurTime + TimeSpan.FromSeconds(DelayDuration);

        Reason = args.Reason;
        UpdateUserInterface(args.Button);
        _adminLogger.Add(LogType.EventStarted, LogImpact.High, $"{ToPrettyString(args.Actor):player} vote for {args.Button}. Reason: {Reason}");

        if (hashSet.Count >= MinCount)
        {
            var ev = new AuthPanelPerformActionEvent(args.Button);
            RaiseLocalEvent(uid, ev);
            _timeout = _timing.CurTime + TimeSpan.FromMinutes(TimeoutDuration);
        }
    }

    public void UpdateUserInterface(AuthPanelAction rawaction)
    {
        if (!Counter.TryGetValue(rawaction, out var hashSet))
            return;

        var action = new AuthPanelConfirmationAction(rawaction, hashSet.Count, MinCount, Reason);

        var query = EntityQueryEnumerator<AuthPanelComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (!_ui.HasUi(uid, AuthPanelUiKey.Key))
                continue;

            var state = new AuthPanelConfirmationActionState(action);

            _ui.SetUiState(uid, AuthPanelUiKey.Key, state);
            _appearance.SetData(uid, AuthPanelVisualLayers.Confirm, true);
        }
    }

    public override void Update(float frameTime)
    {
        if (_delay != null && _timing.CurTime >= _delay)
        {
            _delay = null;
        }

        if (_timeout != null && _timing.CurTime >= _timeout)
        {
            ClearPanel();
        }
    }
}
