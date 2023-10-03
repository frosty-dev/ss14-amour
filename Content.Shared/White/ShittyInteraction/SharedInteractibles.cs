using Content.Shared.White.ShittyInteraction.Interactions;

namespace Content.Shared.White.ShittyInteraction;

public abstract class SharedInteractibles : EntitySystem
{
    public override void Initialize()
    {
        SubscribeAllEvent<EbatEvent>(OnEbat);
        SubscribeAllEvent<EbatEndEvent>(OnEndEbat);
    }

    protected virtual void OnEndEbat(EbatEndEvent ev)
    {
        Logger.Debug(ev.Performer + " end Ebal " + ev.Target);
    }

    protected virtual void OnEbat(EbatEvent ev)
    {
        Logger.Debug(ev.Performer + " Ebal " + ev.Target);
    }
}
