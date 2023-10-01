using Content.Shared.White.Interaction;
using Content.Shared.White.Interaction.InteractionEvent;
using Content.Shared.White.Interaction.Interactions;

namespace Content.Server.White.Interaction;

public sealed class Interactibles : SharedInteractibles
{
    protected override void OnEbat(EbatEvent ev)
    {
        base.OnEbat(ev);
        Logger.Debug("IsServer!");
    }
}
