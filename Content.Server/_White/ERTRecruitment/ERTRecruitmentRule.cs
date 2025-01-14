using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Events;
using Content.Server.StationEvents.Events;
using Content.Server._White.GhostRecruitment;
using Content.Server.GameTicking.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared._White;
using Content.Shared._White.GhostRecruitment;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.Map;

namespace Content.Server._White.ERTRecruitment;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public sealed class ERTRecruitmentRule : StationEventSystem<ERTRecruitmentRuleComponent>
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly GhostRecruitmentSystem _recruitment = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    private ISawmill _logger = default!;

    public bool IsDisabled = false;

    public override void Initialize()
    {
        base.Initialize();

        _logger = Logger.GetSawmill("ERTRecruit");
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
        SubscribeLocalEvent<RecruitedComponent, GhostRecruitmentSuccessEvent>(OnRecruitmentSuccess);
    }

    protected override void Added(EntityUid uid, ERTRecruitmentRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (TryGetRandomStation(out var stationUid))
        {
            component.TargetStation = stationUid;
        }

        if (IsDisabled)
        {
            component.IsBlocked = true;
            return;
        }

        var query = EntityQueryEnumerator<ERTMapComponent>();
        if (query.MoveNext(out uid, out var ertMapComponent))
        {
            component.Outpost = uid;
            //component.Shuttle = ertMapComponent.Shuttle;
            component.MapId = ertMapComponent.MapId;
        }
    }

    protected override void Started(EntityUid uid, ERTRecruitmentRuleComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (component.TargetStation == null)
        {
            ForceEndSelf(uid, gameRule);
            _adminLogger.Add(LogType.EventStarted, LogImpact.High, $"ERT Declined - Target Station is missing");
            return;
        }

        if (component.IsBlocked || IsDisabled)
        {
            DeclineERT(component.TargetStation.Value);
            ForceEndSelf(uid, gameRule);
            _adminLogger.Add(LogType.EventStarted, LogImpact.High, $"ERT Declined - Event disabled");
            return;
        }

        if (_recruitment.GetEventSpawners(ERTRecruitmentRuleComponent.EventName).Count() < component.MinPlayers)
        {
            DeclineERT(component.TargetStation.Value);
            ForceEndSelf(uid, gameRule);
            _adminLogger.Add(LogType.EventStarted, LogImpact.High, $"ERT Declined - Not enough spawners");
            return;
        }

        _chatSystem.DispatchStationAnnouncement(component.TargetStation.Value, Loc.GetString("ert-wait-message"), colorOverride: Color.Gold);

        /*
        if (TryComp<ShuttleComponent>(component.Shuttle, out var shuttle) && component.Outpost != null)
        {
            _shuttle.TryFTLDock(component.Shuttle.Value, shuttle, component.Outpost.Value);
        }
        */

        _recruitment.StartRecruitment(ERTRecruitmentRuleComponent.EventName, component.OverallPlaytime);
    }

    protected override void Ended(EntityUid uid, ERTRecruitmentRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);
        var ertsys = _entities.System<ERTRecruitmentRule>();

        var check1 = component.IsBlocked || ertsys.IsDisabled;

        var check2 = _recruitment.GetAllRecruited(ERTRecruitmentRuleComponent.EventName).Count() < component.MinPlayers;

        if (check1)
        {
            if (component.TargetStation != null)
                DeclineERT(component.TargetStation.Value);
            _adminLogger.Add(LogType.EventStarted, LogImpact.High, $"{"ERT Declined - Event disabled"}");
            _recruitment.Cleanup(ERTRecruitmentRuleComponent.EventName);
            return;
        }
        if (check2)
        {
            if (component.TargetStation != null)
                DeclineERT(component.TargetStation.Value);
            _adminLogger.Add(LogType.EventStarted, LogImpact.High, $"ERT Declined - Not enough ghosts willing to play ERT");
            _recruitment.Cleanup(ERTRecruitmentRuleComponent.EventName);
            return;
        }
        else
        {
            if (component.TargetStation != null)
                AcceptERT(component.TargetStation.Value);

            _recruitment.EndRecruitment(ERTRecruitmentRuleComponent.EventName);
            ertsys.IsDisabled = true;
        }

    }

    private void OnRecruitmentSuccess(EntityUid uid, RecruitedComponent component, GhostRecruitmentSuccessEvent args)
    {
        var ev = new ERTRecruitedReasonEvent();
        RaiseLocalEvent(uid, ev);

        if (args.PlayerSession != null)
        {
            _chat.DispatchServerMessage(args.PlayerSession, Loc.GetString("ert-description"));
            _chat.DispatchServerMessage(args.PlayerSession, Loc.GetString("ert-reason", ("reason", ev.Reason)));
        }
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        // Disabled in dev - Resources/ConfigPresets/Build/development.toml
        if (_cfgManager.GetCVar(WhiteCVars.LoadErtMap))
            SpawnMap();
    }

    public void AcceptERT(EntityUid targetStation)
    {
        _chatSystem.DispatchStationAnnouncement(targetStation, Loc.GetString("ert-accept-message"),
           colorOverride: Color.Gold, announcementSound: ERTRecruitmentRuleComponent.ERTYes);
    }

    public void DeclineERT(EntityUid targetStation)
    {
        _chatSystem.DispatchStationAnnouncement(targetStation, Loc.GetString("ert-deny-message"),
            colorOverride: Color.Gold, announcementSound: ERTRecruitmentRuleComponent.ERTNo);
    }

    private bool SpawnMap()
    {
        _logger.Debug($"Loading maps!");

        var mapId = _mapManager.CreateMap();
        var options = new MapLoadOptions
        {
            LoadMap = true,
        };

        if (!_map.TryLoad(mapId, ERTMapComponent.OutpostMap.ToString(), out var outpostGrids, options) || outpostGrids.Count == 0)
        {
            _logger.Error($"Error loading map {ERTMapComponent.OutpostMap}!");
            return false;
        }
        _logger.Debug($"Loaded map {ERTMapComponent.OutpostMap} on {mapId}!");

        // Assume the first grid is the outpost grid.
        var outpost = outpostGrids[0];

        // Listen I just don't want it to overlap.

        // RinKeeper
        // Now Shuttle is already on Outpost grid, so we dont need that.
        /*
        if (!_map.TryLoad(mapId, ERTMapComponent.ShuttleMap.ToString(), out var grids, new MapLoadOptions {Offset = Vector2.One * 1000f}) || !grids.Any())
        {
            _logger.Error( $"Error loading grid {ERTMapComponent.ShuttleMap}!");
            return false;
        }
        _logger.Debug($"Loaded shuttle {ERTMapComponent.ShuttleMap} on {mapId}!");

        var shuttleId = grids.First();

        // Naughty, someone saved the shuttle as a map.
        if (Deleted(shuttleId))
        {
            _logger.Error( $"Tried to load shuttle as a map, aborting.");
            _mapManager.DeleteMap(mapId);
            return false;
        }
        */

        var ertMap = EnsureComp<ERTMapComponent>(outpost);
        ertMap.MapId = mapId;
        //ERTMap.Shuttle = shuttleId;

        return true;
    }
}
