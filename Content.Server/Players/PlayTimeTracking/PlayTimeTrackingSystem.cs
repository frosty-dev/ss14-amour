using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Afk;
using Content.Server.Afk.Events;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Events;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Players;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Players.PlayTimeTracking;

/// <summary>
/// Connects <see cref="IPlayTimeTrackingManager"/> to the simulation state. Reports trackers and such.
/// </summary>
public sealed class PlayTimeTrackingSystem : EntitySystem
{
    [Dependency] private readonly IAfkManager _afk = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly MindSystem _minds = default!;
    [Dependency] private readonly IPlayTimeTrackingManager _tracking = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IServerPreferencesManager _preferencesManager = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;

    public override void Initialize()
    {
        base.Initialize();

        _tracking.CalcTrackers += CalcTrackers;

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<RoleAddedEvent>(OnRoleAdd);
        SubscribeLocalEvent<RoleRemovedEvent>(OnRoleRemove);
        SubscribeLocalEvent<AFKEvent>(OnAFK);
        SubscribeLocalEvent<UnAFKEvent>(OnUnAFK);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<PlayerJoinedLobbyEvent>(OnPlayerJoinedLobby);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _tracking.CalcTrackers -= CalcTrackers;
    }

    private void CalcTrackers(ICommonSession player, HashSet<string> trackers)
    {
        if (_afk.IsAfk(player))
            return;

        if (!IsPlayerAlive(player))
            return;

        trackers.Add(PlayTimeTrackingShared.TrackerOverall);
        trackers.UnionWith(GetTimedRoles(player));
    }

    private bool IsPlayerAlive(ICommonSession session)
    {
        var attached = session.AttachedEntity;
        if (attached == null)
            return false;

        if (!TryComp<MobStateComponent>(attached, out var state))
            return false;

        return state.CurrentState is MobState.Alive or MobState.Critical;
    }

    public IEnumerable<string> GetTimedRoles(EntityUid mindId)
    {
        var ev = new MindGetAllRolesEvent(new List<RoleInfo>());
        RaiseLocalEvent(mindId, ref ev);

        foreach (var role in ev.Roles)
        {
            if (string.IsNullOrWhiteSpace(role.PlayTimeTrackerId))
                continue;

            yield return _prototypes.Index<PlayTimeTrackerPrototype>(role.PlayTimeTrackerId).ID;
        }
    }

    private IEnumerable<string> GetTimedRoles(ICommonSession session)
    {
        var contentData = _playerManager.GetPlayerData(session.UserId).ContentData();

        if (contentData?.Mind == null)
            return Enumerable.Empty<string>();

        return GetTimedRoles(contentData.Mind.Value);
    }

    private void OnRoleRemove(RoleRemovedEvent ev)
    {
        if (_minds.TryGetSession(ev.Mind, out var session))
            _tracking.QueueRefreshTrackers(session);
    }

    private void OnRoleAdd(RoleAddedEvent ev)
    {
        if (_minds.TryGetSession(ev.Mind, out var session))
            _tracking.QueueRefreshTrackers(session);
    }

    private void OnRoundEnd(RoundRestartCleanupEvent ev)
    {
        _tracking.Save();
    }

    private void OnUnAFK(ref UnAFKEvent ev)
    {
        _tracking.QueueRefreshTrackers(ev.Session);
    }

    private void OnAFK(ref AFKEvent ev)
    {
        _tracking.QueueRefreshTrackers(ev.Session);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        _tracking.QueueRefreshTrackers(ev.Player);
    }

    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        // This doesn't fire if the player doesn't leave their body. I guess it's fine?
        _tracking.QueueRefreshTrackers(ev.Player);
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        if (!TryComp(ev.Target, out ActorComponent? actor))
            return;

        _tracking.QueueRefreshTrackers(actor.PlayerSession);
    }

    private void OnPlayerJoinedLobby(PlayerJoinedLobbyEvent ev)
    {
        _tracking.QueueRefreshTrackers(ev.PlayerSession);
        // Send timers to client when they join lobby, so the UIs are up-to-date.
        _tracking.QueueSendTimers(ev.PlayerSession);
    }

    public bool IsAllowed(ICommonSession player, string role)
    {
        if (!_prototypes.TryIndex<JobPrototype>(role, out var job) ||
            !_cfg.GetCVar(CCVars.GameRoleTimers))
            return true;

        if (!_tracking.TryGetTrackerTimes(player, out var playTimes))
        {
            Log.Error($"Unable to check playtimes {Environment.StackTrace}");
            playTimes = new Dictionary<string, TimeSpan>();
        }

        var isAdmin = _adminManager.IsAdmin(player, true); // WD

        return JobRequirements.TryRequirementsMet(job, playTimes, out _, EntityManager, _prototypes, (HumanoidCharacterProfile?) _preferencesManager.GetPreferences(player.UserId).SelectedCharacter, isAdmin); // WD EDIT
    }

    public HashSet<string> GetDisallowedJobs(ICommonSession player)
    {
        var roles = new HashSet<string>();

        if (!_cfg.GetCVar(CCVars.GameRoleTimers))
            return roles;

        if (!_tracking.TryGetTrackerTimes(player, out var playTimes))
        {
            Log.Error($"Unable to check playtimes {Environment.StackTrace}");
            playTimes = new Dictionary<string, TimeSpan>();
        }

        var isAdmin = _adminManager.IsAdmin(player, true); // WD

        foreach (var job in _prototypes.EnumeratePrototypes<JobPrototype>())
        {
            if (JobRequirements.TryRequirementsMet(job, playTimes, out _, EntityManager, _prototypes, (HumanoidCharacterProfile?) _preferencesManager.GetPreferences(player.UserId).SelectedCharacter, isAdmin)) // WD EDIT
                roles.Add(job.ID);
        }

        return roles;
    }

    public void RemoveDisallowedJobs(NetUserId userId, List<ProtoId<JobPrototype>> jobs)
    {
        if (!_cfg.GetCVar(CCVars.GameRoleTimers))
            return;

        var player = _playerManager.GetSessionById(userId);

        if (!_tracking.TryGetTrackerTimes(player, out var playTimes))
        {
            // Sorry mate but your playtimes haven't loaded.
            Log.Error($"Playtimes weren't ready yet for {player} on roundstart!");
            playTimes ??= new Dictionary<string, TimeSpan>();
        }

        var isAdmin = _adminManager.IsAdmin(player, true); // WD

        for (var i = 0; i < jobs.Count; i++)
        {
            if (_prototypes.TryIndex(jobs[i], out var job)
                && JobRequirements.TryRequirementsMet(job, playTimes, out _, EntityManager, _prototypes, (HumanoidCharacterProfile?) _preferencesManager.GetPreferences(userId).SelectedCharacter, isAdmin)) // WD EDIT
            {
                continue;
            }

            jobs.RemoveSwap(i);
            i--;
        }
    }

    public void PlayerRolesChanged(ICommonSession player)
    {
        _tracking.QueueRefreshTrackers(player);
    }
}
