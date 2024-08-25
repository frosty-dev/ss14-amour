using Content.Server.GameTicking;
using Content.Server.GameTicking.Components;
using Content.Server._Honk.Speech.Components;
using Content.Server._Honk.AspectsSystem.Aspects.Components;
using Content.Server._White.AspectsSystem.Base;
using Content.Shared.Mind.Components;
using Robust.Shared.Random;

namespace Content.Server._Honk.Aspects;

public sealed class CaucasianAccentAspect : AspectSystem<CaucasianAccentAspectComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);
    }

    protected override void Started(EntityUid uid, CaucasianAccentAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        var query = EntityQueryEnumerator<MindContainerComponent>();

        while (query.MoveNext(out var ent, out _))
        {
            EntityManager.EnsureComponent<CaucasianAccentComponent>(ent);
        }
    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<CaucasianAccentAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (!ev.LateJoin)
                return;

            var mob = ev.Mob;
            EntityManager.EnsureComponent<CaucasianAccentComponent>(mob);
        }
    }
}
