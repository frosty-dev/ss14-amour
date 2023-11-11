using Content.Server.Chat.Systems;
using Content.Shared.White.Crawl;

namespace Content.Server.White.Crawl;

public sealed class CrawlSystem : SharedCrawlSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrawlableComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(EntityUid uid, CrawlableComponent component,ref EmoteEvent args)
    {
        switch (args.Emote.ID)
        {
            case "EmoteCrawl":
                EnableCrawl(uid);
                break;
            case "EmoteCrawlUp":
                DisableCrawl(uid);
                break;
        }
    }
}
