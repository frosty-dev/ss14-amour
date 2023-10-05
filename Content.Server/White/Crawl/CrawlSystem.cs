using Content.Server.Carrying;
using Content.Server.Chat.Systems;
using Content.Shared.Carrying;
using Content.Shared.White.Crawl;

namespace Content.Server.White.Crawl;

public sealed class CrawlSystem : SharedCrawlSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrawlableComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<CrawlComponent,CarryDoAfterEvent>(OnDisable,
            before: new[] { typeof(CarryingSystem) });
    }

    private void OnDisable(EntityUid uid, CrawlComponent component, CarryDoAfterEvent args)
    {
        DisableCrawl(uid);
    }

    private void OnEmote(EntityUid uid, CrawlableComponent component,ref EmoteEvent args)
    {
        if(args.Emote.ID == "EmoteCrawl")
            EnableCrawl(uid);
        else if(args.Emote.ID == "EmoteCrawlUp")
            DisableCrawl(uid);
    }
}
