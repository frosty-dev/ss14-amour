using Content.Shared.Buckle;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;

namespace Content.Shared.White.Crawl;

public abstract class SharedCrawlSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly StandingStateSystem _standingStateSystem = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CrawlComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CrawlComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CrawlableComponent, ComponentShutdown>(OnCrawlShutdown);
        SubscribeLocalEvent<CrawlComponent, StoodEvent>(OnStood);
        SubscribeLocalEvent<CrawlComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    private void OnRefresh(EntityUid uid, CrawlComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.WalkSpeedModifier, component.SprintSpeedModifier);
    }

    private void OnInit(EntityUid uid, CrawlComponent component, ComponentInit args)
    {
        if (_buckle.TryUnbuckle(uid, uid) || !_standingStateSystem.Down(uid, true, false))
        {
            DisableCrawl(uid);
            return;
        }

        _speed.RefreshMovementSpeedModifiers(uid);
    }

    private void OnCrawlShutdown(EntityUid uid, CrawlableComponent component, ComponentShutdown args)
    {
        DisableCrawl(uid);
    }

    private void OnStood(EntityUid uid, CrawlComponent component, StoodEvent args)
    {
        if (component.LifeStage == ComponentLifeStage.Stopping)
            return;

        DisableCrawl(uid);
    }

    private void OnShutdown(EntityUid uid, CrawlComponent component, ComponentShutdown args)
    {
        _standingStateSystem.Stand(uid);
        component.SprintSpeedModifier = 1f;
        component.WalkSpeedModifier = 1f;
        _speed.RefreshMovementSpeedModifiers(uid);
    }

    public void EnableCrawl(EntityUid uid)
    {
        EnsureComp<CrawlComponent>(uid);
    }

    public void DisableCrawl(EntityUid uid)
    {
        RemComp<CrawlComponent>(uid);
    }
}
