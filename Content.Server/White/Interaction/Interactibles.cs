using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.White.Anus;
using Content.Server.White.Cunt;
using Content.Shared.Chat;
using Content.Shared.White.ShittyInteraction;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Server.GameObjects;

namespace Content.Server.White.Interaction;

public sealed class Interactibles : SharedInteractibles
{
    [Dependency] private readonly AnusSystem _anus = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly CuntSystem _cunt = default!;

    protected override void OnEbat(EbatEvent ev)
    {
        base.OnEbat(ev);

        if (!_anus.HasAccessToButt(ev.Performer))
        {
            SpellSomeShit(ev.Performer,"interaction-performer-ogurec-no");
            ev.Cancel();
            return;
        }

        if (!_anus.HasAccessToAnus(ev.Target))
        {
            SpellSomeShit(ev.Performer,"interaction-target-anus-no");
            ev.Cancel();
            return;
        }
    }

    protected override void OnEndEbat(EbatEndEvent ev)
    {
        base.OnEndEbat(ev);

        _cunt.GenCum(ev.Performer,10);
        _cunt.TryCunt(ev.Performer);
    }

    protected void SpellSomeShit(EntityUid uid, string message)
    {
        message = Loc.GetString(message);
        if(TryComp<ActorComponent>(uid,out var actor))
            _chatManager.ChatMessageToOne(ChatChannel.Emotes,message,message,EntityUid.Invalid, false, actor.PlayerSession.ConnectedClient);

    }
}
