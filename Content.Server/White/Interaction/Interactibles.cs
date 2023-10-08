using Content.Server.Chat.Managers;
using Content.Server.Pulling;
using Content.Server.White.Anus;
using Content.Server.White.Crawl;
using Content.Server.White.Cunt;
using Content.Shared.Chat;
using Content.Shared.White.Anus;
using Content.Shared.White.ShittyInteraction;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Server.GameObjects;

namespace Content.Server.White.Interaction;

public sealed class Interactibles : SharedInteractibles
{
    [Dependency] private readonly CuntSystem _cunt = default!;
    [Dependency] private readonly CrawlSystem _crawl = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawledEvent>(OnCrawl);
        SubscribeLocalEvent<PullTargetEvent>(OnPull);
    }

    private void OnPull(PullTargetEvent ev)
    {
        _pulling.TryStartPull(ev.Performer, ev.Target);
    }

    private void OnCrawl(CrawledEvent ev)
    {
        if(!ev.Cancelled)
            _crawl.EnableCrawl(ev.Target);
    }

    protected override void OnEbat(EbatEvent ev)
    {
        base.OnEbat(ev);

        EnsureComp<MoanComponent>(ev.Target);
    }

    protected override void OnEndEbat(EbatEndEvent ev)
    {
        base.OnEndEbat(ev);

        _cunt.GenCum(ev.Performer,10);
        _cunt.TryCunt(ev.Performer);
        RemComp<MoanComponent>(ev.Target);
    }
}
