using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Content.Server._Honk.GameTicking.Rules.Components;
using Content.Server.GameTicking.Components;
using Content.Server.StationEvents.Events;

namespace Content.Server._Honk.GameTicking.Rules;

public sealed class DemolitionistRuleSystem : StationEventSystem<DemolitionistRuleComponent>
{
    [Dependency] private readonly MapLoaderSystem _map = default!;

    protected override void Started(EntityUid uid, DemolitionistRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);


        var shuttleMap = MapManager.CreateMap();
        var options = new MapLoadOptions
        {
            LoadMap = true,
        };
        _map.TryLoad(shuttleMap, component.DemolitionistShuttlePath, out _, options);

    }
}
