using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;

namespace Content.Shared.White.Crawl;

public abstract class SharedCrawlSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CrawlComponent,ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CrawlComponent,ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CrawlableComponent,ComponentStartup>(OnCrawlStart);
        SubscribeLocalEvent<CrawlableComponent,CrawlToggledEvent>(OnToggled);

    }

    private void OnToggled(EntityUid uid, CrawlableComponent component, CrawlToggledEvent args)
    {
       ToggleCrawl(uid);
    }

    private void OnCrawlStart(EntityUid uid, CrawlableComponent component, ComponentStartup args)
    {
        if(!_prototypeManager.TryIndex<InstantActionPrototype>("Crawl", out var crawlAction))
            return;

        _actionsSystem.AddAction(uid,new InstantAction(crawlAction),null);
    }

    private void OnShutdown(EntityUid uid, CrawlComponent component, ComponentShutdown args)
    {
        RemComp<KnockedDownComponent>(uid);
        if (TryComp<MovementSpeedModifierComponent>(uid, out var mod))
        {
            _speed.ChangeBaseSpeed(uid,component.WalkSpeed,component.SpringSpeed,mod.Acceleration,mod);
            Dirty(mod);
        }
    }

    private void OnStartup(EntityUid uid, CrawlComponent component, ComponentStartup args)
    {
        _statusEffectsSystem.TryRemoveStatusEffect(uid, "KnockedDown");
        EnsureComp<KnockedDownComponent>(uid);

        if (TryComp<MovementSpeedModifierComponent>(uid, out var mod))
        {
            component.SpringSpeed = mod.BaseSprintSpeed;
            component.WalkSpeed = mod.BaseWalkSpeed;
        }

        var modifierComponent = EnsureComp<MovementSpeedModifierComponent>(uid);
        _speed.ChangeBaseSpeed(uid,1,2,modifierComponent.Acceleration,modifierComponent);
    }

    public void ToggleCrawl(EntityUid uid)
    {
        if (!HasComp<CrawlComponent>(uid))
            EnableCrawl(uid);
        else
            DisableCrawl(uid);
    }

    public void EnableCrawl(EntityUid uid)
    {
        if(!HasComp<KnockedDownComponent>(uid))
            EnsureComp<CrawlComponent>(uid);
    }

    public void DisableCrawl(EntityUid uid)
    {
        RemComp<CrawlComponent>(uid);
    }
}

public sealed class CrawlToggledEvent : InstantActionEvent
{
}
