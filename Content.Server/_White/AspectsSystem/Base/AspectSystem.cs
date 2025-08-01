using System.Diagnostics.CodeAnalysis;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Station.Components;
using Content.Shared.Database;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._White.AspectsSystem.Base;

/// <summary>
/// Base class for aspect systems.
/// </summary>
/// <typeparam name="T">The type of component to which the system is applied.</typeparam>
public abstract class AspectSystem<T> : GameRuleSystem<T> where T : Component
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    protected ISawmill Sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        Sawmill = Logger.GetSawmill("aspects");
    }

    /// <summary>
    /// Called every tick when this aspect is running.
    /// </summary>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var aspect, out var ruleData))
        {
            if (!GameTicker.IsGameRuleAdded(uid, ruleData))
                continue;

            if (!GameTicker.IsGameRuleActive(uid, ruleData) && _timing.CurTime >= aspect.StartTime)
            {
                GameTicker.StartGameRule(uid, ruleData);
            }
        }
    }

    /// <summary>
    /// Called when an aspect is added to an entity.
    /// </summary>
    protected override void Added(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (!TryComp<AspectComponent>(uid, out var aspect))
            return;

        _adminLogManager.Add(LogType.AspectAnnounced, $"Aspect added {ToPrettyString(uid)}");

        if (aspect is { Description: not null, IsHidden: false })
        {
            _chatSystem.DispatchGlobalAnnouncement(aspect.Description,
            sender: aspect.Sender ??= "Центральное Командование",
            playSound: false,
            colorOverride: Color.Aquamarine);
        }

        _audio.PlayGlobal(aspect.StartAudio, Filter.Broadcast(), true);
        aspect.StartTime = _timing.CurTime + aspect.StartDelay;
    }

    /// <summary>
    /// Called when an aspect is started.
    /// </summary>
    protected override void Started(
        EntityUid uid,
        T component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryComp<AspectComponent>(uid, out _))
            return;

        _adminLogManager.Add(LogType.AspectStarted, LogImpact.High, $"Aspect started: {ToPrettyString(uid)}");
    }

    /// <summary>
    /// Called when an aspect is ended.
    /// </summary>
    protected override void Ended(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if (!TryComp<AspectComponent>(uid, out var aspect))
            return;

        _adminLogManager.Add(LogType.AspectStopped, $"Aspect ended: {ToPrettyString(uid)}");

        if (aspect is { Name: not null, IsHidden: false })
        {

            _chatSystem.DispatchGlobalAnnouncement($"Именем аспекта являлось: {aspect.Name}",
            sender: aspect.Sender ??= "Центральное Командование",
            playSound: false,
            colorOverride: Color.Aquamarine);
        }

        _audio.PlayGlobal(aspect.EndAudio, Filter.Broadcast(), true);
    }

    #region Helpers

    /// <summary>
    /// Forces this aspect to end prematurely.
    /// </summary>
    /// <param name="uid">The entity UID on which the aspect is being performed.</param>
    /// <param name="component">The game rule component associated with this aspect (optional).</param>
    protected void ForceEndSelf(EntityUid uid, GameRuleComponent? component = null)
    {
        GameTicker.EndGameRule(uid, component);
    }

    protected bool TryGetStationGrids(
        [NotNullWhen(true)] out EntityUid? targetStation,
        [NotNullWhen(true)] out HashSet<EntityUid>? grids)
    {
        if (!TryGetRandomStation(out targetStation))
        {
            targetStation = EntityUid.Invalid;
            grids = null;
            return false;
        }

        grids = Comp<StationDataComponent>(targetStation.Value).Grids;

        return grids.Count > 0;
    }

    #endregion
}
