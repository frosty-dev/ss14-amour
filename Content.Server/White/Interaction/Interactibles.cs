using Content.Server.Chat.Managers;
using Content.Server.White.Anus;
using Content.Server.White.Crawl;
using Content.Server.White.Cunt;
using Content.Shared.Chat;
using Content.Shared.White.ShittyInteraction;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Server.GameObjects;

namespace Content.Server.White.Interaction;

public sealed class Interactibles : SharedInteractibles
{
    [Dependency] private readonly AnusSystem _anus = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly CuntSystem _cunt = default!;
    [Dependency] private readonly CrawlSystem _crawl = default!;
    [Dependency] private readonly InteractibleSystem _interactible = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawledEvent>(OnCrawl);
    }

    protected override void OnShlifovka(ShlifovkaEvent ev)
    {
        base.OnShlifovka(ev);

        if (!_anus.HasAccessToButt(ev.Target))
        {
            SpellSomeShit(ev.Performer,"interaction-performer-ogurec-no-oth");
            ev.Cancel();
            return;
        }

        //_rotateTo.TryFaceCoordinates(ev.Performer, Transform(ev.Target).LocalPosition);
    }

    protected override void OnShlep(ShlepButtEvent ev)
    {
        base.OnShlep(ev);
    }

    private void OnCrawl(CrawledEvent ev)
    {
        _crawl.EnableCrawl(ev.Target);
    }

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
        var gender = _interactible.GetGender(uid);

        message = Loc.GetString(message,("gender",gender));
        if(TryComp<ActorComponent>(uid,out var actor))
        {
            _chatManager.ChatMessageToOne(ChatChannel.Emotes,message,message,EntityUid.Invalid,
                false, actor.PlayerSession.ConnectedClient);
        }
    }


}
