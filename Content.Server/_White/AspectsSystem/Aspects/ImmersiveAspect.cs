using System.Numerics;
using Content.Server.GameTicking.Rules.Components;
using Content.Server._White.AspectsSystem.Aspects.Components;
using Content.Server._White.AspectsSystem.Base;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Components;
using Content.Shared._White.Telescope;
using Content.Shared.Humanoid;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Server._White.AspectsSystem.Aspects;

public sealed class ImmersiveAspect : AspectSystem<ImmersiveAspectComponent>
{

    [Dependency] private readonly SharedContentEyeSystem _eye = default!;
    [Dependency] private readonly SharedTelescopeSystem _telescope = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);
    }

    protected override void Started(EntityUid uid, ImmersiveAspectComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        WayToHRPPlusPlus();
    }

    private void WayToHRPPlusPlus()
    {
        var humans = EntityQuery<HumanoidAppearanceComponent>();

        foreach (var human in humans)
        {
            var entity = human.Owner;

            if (!HasComp<ContentEyeComponent>(entity))
                continue;

            FuckUpEye(entity, 0.6f);
            AddTelescope(entity);
        }
    }

    private void FuckUpEye(EntityUid human, float modifier)
    {
        _eye.SetMaxZoom(human, new Vector2(modifier));
        _eye.SetZoom(human, new Vector2(modifier));
    }

    private void AddTelescope(EntityUid human)
    {
        var telescope = EnsureComp<TelescopeComponent>(human);

        _telescope.SetParameters((human, telescope), 0.15f, 0.07f);
    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        if (!ev.LateJoin)
            return;

        if (!HasComp<ContentEyeComponent>(ev.Mob))
            return;

        var query = EntityQueryEnumerator<ImmersiveAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            FuckUpEye(ev.Mob, 0.6f);
            AddTelescope(ev.Mob);
        }
    }


    protected override void Ended(EntityUid uid, ImmersiveAspectComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        var humans = EntityQuery<HumanoidAppearanceComponent>();

        foreach (var human in humans)
        {
            var entity = human.Owner;

            if (!HasComp<ContentEyeComponent>(entity))
                continue;

            FuckUpEye(entity, 1f);

            RemComp<TelescopeComponent>(entity);
        }
    }
}
