using Content.Shared.White.ShittyInteraction;
using Content.Shared.White.ShittyInteraction.Interactions;

namespace Content.Server.White.Interaction;

public sealed class Interactibles : SharedInteractibles
{
    protected override void OnEbat(EbatEvent ev)
    {
        base.OnEbat(ev);
        Logger.Debug("IsServer!");
    }

    protected override void OnEndEbat(EbatEndEvent ev)
    {
        base.OnEndEbat(ev);
        Logger.Debug("IsServer Yay!");
    }
}
