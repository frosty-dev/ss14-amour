using Content.Shared.Interaction.Events;
using Robust.Shared.GameStates;

namespace Content.Shared.White.Dildo;

public abstract class SharedDildoSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<DildoComponent,ComponentGetState>(OnGetState);
        SubscribeLocalEvent<DildoComponent,ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, DildoComponent component, ref ComponentHandleState args)
    {
        if(args.Current is not DildoComponentState state)
            return;

        component.IsVibrating = state.IsVibrating;
        ToggleVibrate(uid,component);
    }

    private void OnGetState(EntityUid uid, DildoComponent component,ref ComponentGetState args)
    {
        args.State = new DildoComponentState(component.IsVibrating);
    }

    public void ToggleVibration(EntityUid uid, DildoComponent? component = null)
    {
        if(!Resolve(uid,ref component))
            return;

        component.IsVibrating = !component.IsVibrating;
        Dirty(component);
    }

    public virtual void ToggleVibrate(EntityUid uid, DildoComponent component)
    {
    }
}
