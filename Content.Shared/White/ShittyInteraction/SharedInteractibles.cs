using Content.Shared.White.Anus;
using Content.Shared.White.ShittyInteraction.Interactions;

namespace Content.Shared.White.ShittyInteraction;

public abstract class SharedInteractibles : EntitySystem
{
    public override void Initialize()
    {
        SubscribeAllEvent<EbatEvent>(OnEbat);
        SubscribeAllEvent<EbatEndEvent>(OnEndEbat);

        SubscribeAllEvent<ShlepButtEvent>(OnShlep);
        SubscribeAllEvent<ShlifovkaEvent>(OnShlifovka);
        SubscribeAllEvent<ComeToTargetEvent>(OnCome);
    }

    protected virtual void OnCome(ComeToTargetEvent ev)
    {
    }

    protected virtual void OnShlifovka(ShlifovkaEvent ev)
    {
    }

    protected virtual void OnShlep(ShlepButtEvent ev)
    {
    }

    protected virtual void OnEndEbat(EbatEndEvent ev)
    {
        RemComp<MoanComponent>(ev.Target);
    }

    protected virtual void OnEbat(EbatEvent ev)
    {
        EnsureComp<MoanComponent>(ev.Target);
    }
}
