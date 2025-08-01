using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server._White.Animations;
using Content.Server._White.AspectsSystem.Aspects.Components;
using Content.Server._White.AspectsSystem.Base;
using Content.Server.GameTicking.Components;
using Content.Shared.Mobs.Components;
using Content.Shared._White.Animations;

namespace Content.Server._White.AspectsSystem.Aspects;

public sealed class DancingAspect : AspectSystem<DancingAspectComponent>
{
    [Dependency] private readonly ChatHelper _chatHelper = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);
    }

    protected override void Started(
        EntityUid uid,
        DancingAspectComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        var query = EntityQueryEnumerator<EmoteAnimationComponent, MobStateComponent>();
        while (query.MoveNext(out var ent, out _, out _))
        {
            EnsureComp<DancingComponent>(ent);
        }
    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<DancingAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (!ev.LateJoin)
                return;

            var mob = ev.Mob;

            EnsureComp<DancingComponent>(mob);
            _chatHelper.SendAspectDescription(mob, Loc.GetString("dancing-aspect-desc"));
        }
    }
}
