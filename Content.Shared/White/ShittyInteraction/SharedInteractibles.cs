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
    }

    protected virtual void OnEbat(EbatEvent ev)
    {
    }
}
