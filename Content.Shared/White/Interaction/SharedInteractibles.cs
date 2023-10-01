using Content.Shared.White.Interaction.InteractionEvent;
using Content.Shared.White.Interaction.Interactions;

namespace Content.Shared.White.Interaction;

public abstract class SharedInteractibles : EntitySystem
{
    public override void Initialize()
    {
        SubscribeAllEvent<EbatEvent>(OnEbat);
    }

    protected virtual void OnEbat(EbatEvent ev)
    {
        Logger.Debug(ev.Performer + " Ebal " + ev.Target);
    }
}
