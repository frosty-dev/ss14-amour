using Robust.Shared.GameStates;

namespace Content.Shared._Amour.Vibrator;

public abstract class SharedVibratorSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<VibratorComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<VibratorComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, VibratorComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not VibratorComponentState state)
            return;

        component.IsVibrating = state.IsVibrating;
        ToggleVibrate(uid, component);
    }

    private void OnGetState(EntityUid uid, VibratorComponent component, ref ComponentGetState args)
    {
        args.State = new VibratorComponentState(component.IsVibrating);
    }

    public void ToggleVibration(EntityUid uid, VibratorComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.IsVibrating = !component.IsVibrating;
        Dirty(uid, component);
    }

    public virtual void ToggleVibrate(EntityUid uid, VibratorComponent component)
    {
    }
}
