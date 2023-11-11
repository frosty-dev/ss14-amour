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
        SubscribeLocalEvent<CrawlComponent,ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CrawlComponent,ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CrawlableComponent,ComponentShutdown>(OnCrawlShutdown);
        SubscribeLocalEvent<CrawlComponent,StoodEvent>(OnStood);
    }

    private void OnCrawlShutdown(EntityUid uid, CrawlableComponent component, ComponentShutdown args)
    {
        DisableCrawl(uid);
    }

    private void OnStood(EntityUid uid, CrawlComponent component, StoodEvent args)
    {
        if(component.LifeStage == ComponentLifeStage.Stopping)
            return;

        DisableCrawl(uid);
    }

    private void OnShutdown(EntityUid uid, CrawlComponent component, ComponentShutdown args)
    {
        _standingStateSystem.Stand(uid);
        if (TryComp<MovementSpeedModifierComponent>(uid, out var mod))
        {
            _speed.ChangeBaseSpeed(uid,component.WalkSpeed,component.SpringSpeed,mod.Acceleration,mod);
            Dirty(mod);
        }
    }

    private void OnStartup(EntityUid uid, CrawlComponent component, ComponentStartup args)
    {
        _statusEffectsSystem.TryRemoveStatusEffect(uid, "KnockedDown");

        if (TryComp<MovementSpeedModifierComponent>(uid, out var mod))
        {
            component.SpringSpeed = mod.BaseSprintSpeed;
            component.WalkSpeed = mod.BaseWalkSpeed;
        }

        var modifierComponent = EnsureComp<MovementSpeedModifierComponent>(uid);
        _speed.ChangeBaseSpeed(uid,1,2,modifierComponent.Acceleration,modifierComponent);

        if (_buckle.TryUnbuckle(uid, uid) || !_standingStateSystem.Down(uid))
        {
            DisableCrawl(uid);
        }
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
