using System.Linq;
using Content.Server.Antag.Components;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Roles;
using Content.Server.Roles.Jobs;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Systems;
using Content.Shared.Antag;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.Players;
using Content.Shared.Preferences;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Content.Server._Miracle.GulagSystem;
using Content.Server._White.Sponsors;
using Content.Server.Inventory;
using FastAccessors;
using Robust.Shared.Utility;

namespace Content.Server.Antag;

public sealed partial class AntagSelectionSystem : GameRuleSystem<AntagSelectionComponent>
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IServerPreferencesManager _pref = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;
    [Dependency] private readonly JobSystem _jobs = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly GulagSystem _gulag = default!; // WD
    [Dependency] private readonly ServerInventorySystem _inventory = default!; // WD
    [Dependency] private readonly SponsorsManager _sponsors = default!; // WD

    // arbitrary random number to give late joining some mild interest.
    public const float LateJoinRandomChance = 0.5f;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostRoleAntagSpawnerComponent, TakeGhostRoleEvent>(OnTakeGhostRole);

        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnPlayerSpawning);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnJobsAssigned);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
    }

    private void OnTakeGhostRole(Entity<GhostRoleAntagSpawnerComponent> ent, ref TakeGhostRoleEvent args)
    {
        if (args.TookRole)
            return;

        if (ent.Comp.Rule is not { } rule || ent.Comp.Definition is not { } def)
            return;

        if (!Exists(rule) || !TryComp<AntagSelectionComponent>(rule, out var select))
            return;

        MakeAntag((rule, select), args.Player, def, ignoreSpawner: true);
        args.TookRole = true;
        _ghostRole.UnregisterGhostRole((ent, Comp<GhostRoleComponent>(ent)));
    }

    private void OnPlayerSpawning(RulePlayerSpawningEvent args)
    {
        var pool = args.PlayerPool;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var comp, out _))
        {
            if (comp.SelectionTime != AntagSelectionTime.PrePlayerSpawn)
                continue;

            if (comp.SelectionsComplete)
                continue;

            ChooseAntags((uid, comp), pool);

            foreach (var session in comp.SelectedSessions)
            {
                args.PlayerPool.Remove(session);
                GameTicker.PlayerJoinGame(session);
            }
        }
    }

    private void OnJobsAssigned(RulePlayerJobsAssignedEvent args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var comp, out _))
        {
            if (comp.SelectionTime != AntagSelectionTime.PostPlayerSpawn)
                continue;

            ChooseAntags((uid, comp), args.Players);
        }
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (!args.LateJoin)
            return;

        // TODO: this really doesn't handle multiple latejoin definitions well
        // eventually this should probably store the players per definition with some kind of unique identifier.
        // something to figure out later.

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var antag, out _))
        {
            // TODO ANTAG
            // what why aasdiuhasdopiuasdfhksad
            // stop this insanity please
            // probability of antag assignment shouldn't depend on the order in which rules are returned by the query.
            if (!RobustRandom.Prob(LateJoinRandomChance))
                continue;

            if (!antag.Definitions.Any(p => p.LateJoinAdditional))
                continue;

            DebugTools.AssertEqual(antag.SelectionTime, AntagSelectionTime.PostPlayerSpawn);

            if (!TryGetNextAvailableDefinition((uid, antag), out var def))
                continue;

            if (TryMakeAntag((uid, antag), args.Player, def.Value))
                break;
        }
    }

    protected override void Added(EntityUid uid, AntagSelectionComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        for (var i = 0; i < component.Definitions.Count; i++)
        {
            var def = component.Definitions[i];

            if (def.MinRange != null)
            {
                def.Min = def.MinRange.Value.Next(RobustRandom);
            }

            if (def.MaxRange != null)
            {
                def.Max = def.MaxRange.Value.Next(RobustRandom);
            }
        }
    }

    protected override void Started(EntityUid uid, AntagSelectionComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // If the round has not yet started, we defer antag selection until roundstart
        if (GameTicker.RunLevel != GameRunLevel.InRound)
            return;

        if (component.SelectionsComplete)
            return;

        var players = _playerManager.Sessions
            .Where(x => GameTicker.PlayerGameStatuses[x.UserId] == PlayerGameStatus.JoinedGame)
            .ToList();

        ChooseAntags((uid, component), players);
    }

    /// <summary>
    /// Chooses antagonists from the given selection of players
    /// </summary>
    public void ChooseAntags(Entity<AntagSelectionComponent> ent, IList<ICommonSession> pool)
    {
        if (ent.Comp.SelectionsComplete)
            return;

        foreach (var def in ent.Comp.Definitions)
        {
            ChooseAntags(ent, pool, def);
        }

        ent.Comp.SelectionsComplete = true;
    }

    /// <summary>
    /// Chooses antagonists from the given selection of players for the given antag definition.
    /// </summary>
    public void ChooseAntags(Entity<AntagSelectionComponent> ent, IList<ICommonSession> pool, AntagSelectionDefinition def)
    {
        var playerPool = GetPlayerPool(ent, pool, def);
        var count = GetTargetAntagCount(ent, GetTotalPlayerCount(pool), def);

        for (var i = 0; i < count; i++)
        {
            var session = (ICommonSession?) null;

            if (def.PickPlayer)
            {
                if (!playerPool.TryPickAndTake(RobustRandom, out session))
                    break;

                if (ent.Comp.SelectedSessions.Contains(session))
                    continue;
            }

            MakeAntag(ent, session, def);
        }
    }

    /// <summary>
    /// Tries to makes a given player into the specified antagonist.
    /// </summary>
    public bool TryMakeAntag(Entity<AntagSelectionComponent> ent, ICommonSession? session, AntagSelectionDefinition def, bool ignoreSpawner = false)
    {
        if (!IsSessionValid(ent, session, def) ||
            !IsEntityValid(session?.AttachedEntity, def))
        {
            return false;
        }

        MakeAntag(ent, session, def, ignoreSpawner);
        return true;
    }

    /// <summary>
    /// Makes a given player into the specified antagonist.
    /// </summary>
    public void MakeAntag(Entity<AntagSelectionComponent> ent, ICommonSession? session, AntagSelectionDefinition def, bool ignoreSpawner = false)
    {
        var antagEnt = (EntityUid?) null;
        var isSpawner = false;

        if (session != null)
        {
            ent.Comp.SelectedSessions.Add(session);

            // we shouldn't be blocking the entity if they're just a ghost or smth.
            if (!HasComp<GhostComponent>(session.AttachedEntity))
                antagEnt = session.AttachedEntity;
        }
        else if (!ignoreSpawner && def.SpawnerPrototype != null) // don't add spawners if we have a player, dummy.
        {
            antagEnt = Spawn(def.SpawnerPrototype);
            isSpawner = true;
        }

        if (!antagEnt.HasValue)
        {
            var getEntEv = new AntagSelectEntityEvent(session, ent);
            RaiseLocalEvent(ent, ref getEntEv, true);

            if (!getEntEv.Handled)
            {
                throw new InvalidOperationException($"Attempted to make {session} antagonist in gamerule {ToPrettyString(ent)} but there was no valid entity for player.");
            }

            antagEnt = getEntEv.Entity;
        }

        if (antagEnt is not { } player)
            return;

        // WD edit start - preventing wizard with previous entity`s gear
        if (def.DropInventory)
            _inventory.DropAnything(player);
        // WD edit end

        var getPosEv = new AntagSelectLocationEvent(session, ent);
        RaiseLocalEvent(ent, ref getPosEv, true);
        if (getPosEv.Handled)
        {
            var playerXform = Transform(player);
            var pos = RobustRandom.Pick(getPosEv.Coordinates);
            _transform.SetMapCoordinates((player, playerXform), pos);
        }

        // If we want to just do a ghost role spawner, set up data here and then return early.
        // This could probably be an event in the future if we want to be more refined about it.
        if (isSpawner)
        {
            if (!TryComp<GhostRoleAntagSpawnerComponent>(player, out var spawnerComp))
            {
                Log.Error($"Antag spawner {player} does not have a GhostRoleAntagSpawnerComponent.");
                return;
            }

            spawnerComp.Rule = ent;
            spawnerComp.Definition = def;
            return;
        }

        // The following is where we apply components, equipment, and other changes to our antagonist entity.
        EntityManager.AddComponents(player, def.Components);
        _stationSpawning.EquipStartingGear(player, def.StartingGear);

        if (session != null)
        {
            var curMind = session.GetMind();
            if (curMind == null)
            {
                curMind = _mind.CreateMind(session.UserId, Name(antagEnt.Value));
                _mind.SetUserId(curMind.Value, session.UserId);
            }

            _mind.TransferTo(curMind.Value, antagEnt, ghostCheckOverride: true);
            _role.MindAddRoles(curMind.Value, def.MindComponents);
            ent.Comp.SelectedMinds.Add((curMind.Value, Name(player)));
            SendBriefing(session, def.Briefing);
        }

        var afterEv = new AfterAntagEntitySelectedEvent(session, player, ent, def);
        RaiseLocalEvent(ent, ref afterEv, true);
    }

    /// <summary>
    /// Gets an ordered player pool based on player preferences and the antagonist definition.
    /// </summary>
    public AntagSelectionPlayerPool GetPlayerPool(Entity<AntagSelectionComponent> ent, IList<ICommonSession> sessions, AntagSelectionDefinition def)
    {
        var premiumPool = new List<ICommonSession>();
        var defaultPool = new List<ICommonSession>();

        foreach (var session in sessions)
        {
            if (!IsSessionValid(ent, session, def) ||
                !IsEntityValid(session.AttachedEntity, def))
                continue;

            var pref = (HumanoidCharacterProfile) _pref.GetPreferences(session.UserId).SelectedCharacter;
            var hasPreference = def.PrefRoles.Count != 0 && pref.AntagPreferences.Any(p => def.PrefRoles.Contains(p));
            var hasFallbackPreference = def.FallbackRoles.Count != 0 && pref.AntagPreferences.Any(p => def.FallbackRoles.Contains(p));

            if (RobustRandom.Prob(GetPremiumPoolChance(session)))
            {
                if (hasPreference || hasFallbackPreference)
                    premiumPool.Add(session);
            }
            else
            {
                if (hasPreference)
                    defaultPool.Add(session);
                else if (hasFallbackPreference)
                    defaultPool.Add(session);
            }
        }


        if (premiumPool.Count == 0 && defaultPool.Count == 0)
        {
            foreach (var session in sessions)
            {
                if (IsSessionValid(ent, session, def) && IsEntityValid(session.AttachedEntity, def))
                {
                    defaultPool.Add(session);
                }
            }
        }

        return new AntagSelectionPlayerPool(new() { premiumPool, defaultPool });
    }

    /// <summary>
    /// Checks if a given session is valid for an antagonist.
    /// </summary>
    public bool IsSessionValid(Entity<AntagSelectionComponent> ent, ICommonSession? session, AntagSelectionDefinition def, EntityUid? mind = null)
    {
        if (session == null)
            return true;

        if (session.Status is SessionStatus.Disconnected or SessionStatus.Zombie)
            return false;

        if (ent.Comp.SelectedSessions.Contains(session))
            return false;

        if (_gulag.IsUserGulagged(session.UserId, out _)) // WD
            return false;

        mind ??= session.GetMind();

        // If the player has not spawned in as any entity (e.g., in the lobby), they can be given an antag role/entity.
        if (mind == null)
            return true;

        //todo: we need some way to check that we're not getting the same role twice. (double picking thieves or zombies through midrounds)

        switch (def.MultiAntagSetting)
        {
            case AntagAcceptability.None:
            {
                if (_role.MindIsAntagonist(mind))
                    return false;
                break;
            }
            case AntagAcceptability.NotExclusive:
            {
                if (_role.MindIsExclusiveAntagonist(mind))
                    return false;
                break;
            }
        }

        // todo: expand this to allow for more fine antag-selection logic for game rules.
        if (!_jobs.CanBeAntag(session))
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a given entity (mind/session not included) is valid for a given antagonist.
    /// </summary>
    public bool IsEntityValid(EntityUid? entity, AntagSelectionDefinition def)
    {
        // If the player has not spawned in as any entity (e.g., in the lobby), they can be given an antag role/entity.
        if (entity == null)
            return true;

        if (HasComp<PendingClockInComponent>(entity))
            return false;

        if (!def.AllowNonHumans && !HasComp<HumanoidAppearanceComponent>(entity))
            return false;

        if (def.Whitelist != null)
        {
            if (!def.Whitelist.IsValid(entity.Value, EntityManager))
                return false;
        }

        if (def.Blacklist != null)
        {
            if (def.Blacklist.IsValid(entity.Value, EntityManager))
                return false;
        }

        return true;
    }

    public float GetPremiumPoolChance(ICommonSession session)
    {
        if (!_sponsors.TryGetInfo(session.UserId, out var info))
            return 0f;

        var antagChance = info.AntagChance;

        // Ensure antagChance is within 0-100 range
        antagChance = Math.Clamp(antagChance, 0, 100);

        // Convert clamped antagChance from 0-100 range to 0-1 range
        return antagChance / 100f;
    }
}

/// <summary>
/// Event raised on a game rule entity in order to determine what the antagonist entity will be.
/// Only raised if the selected player's current entity is invalid.
/// </summary>
[ByRefEvent]
public record struct AntagSelectEntityEvent(ICommonSession? Session, Entity<AntagSelectionComponent> GameRule)
{
    public readonly ICommonSession? Session = Session;

    public bool Handled => Entity != null;

    public EntityUid? Entity;
}

/// <summary>
/// Event raised on a game rule entity to determine the location for the antagonist.
/// </summary>
[ByRefEvent]
public record struct AntagSelectLocationEvent(ICommonSession? Session, Entity<AntagSelectionComponent> GameRule)
{
    public readonly ICommonSession? Session = Session;

    public bool Handled => Coordinates.Any();

    public List<MapCoordinates> Coordinates = new();
}

/// <summary>
/// Event raised on a game rule entity after the setup logic for an antag is complete.
/// Used for applying additional more complex setup logic.
/// </summary>
[ByRefEvent]
public readonly record struct AfterAntagEntitySelectedEvent(ICommonSession? Session, EntityUid EntityUid, Entity<AntagSelectionComponent> GameRule, AntagSelectionDefinition Def);

