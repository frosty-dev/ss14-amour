using Content.Shared.Alert;

namespace Content.Shared._Amour.Arousal;

public sealed class ArousalSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArousalComponent,ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, ArousalComponent component, ComponentInit args)
    {
        UpdateAlarm(new Entity<ArousalComponent?>(uid,component));
    }

    private void UpdateAlarm(Entity<ArousalComponent?> entity)
    {
        if(!Resolve(entity,ref entity.Comp))
            return;

        if (entity.Comp.Arousal is > 100 or < 0)
        {
            entity.Comp.Arousal = 0;
            RaiseLocalEvent(entity,new ArousalSplash(new Entity<ArousalComponent>(entity,entity.Comp)));
        }
        RaiseLocalEvent(entity,new ArousalUpdated(new Entity<ArousalComponent>(entity,entity.Comp)));

        _alerts.ShowAlert(entity,AlertType.Arousal,(short)(entity.Comp.Arousal/10));
    }

    public void SetArousal(Entity<ArousalComponent?> entity, short count)
    {
        if(!Resolve(entity,ref entity.Comp))
            return;

        entity.Comp.Arousal = count;
        UpdateAlarm(entity);
    }

    public void AddArousal(Entity<ArousalComponent?> entity, short count)
    {
        if(!Resolve(entity,ref entity.Comp))
            return;

        SetArousal(entity,(short) (entity.Comp.Arousal + count));
    }
}
