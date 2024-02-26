using System.Numerics;
using Content.Server._Amour.Animation;
using Content.Server._Amour.Crawl;
using Content.Server.Pulling;
using Content.Shared._Amour.Animation;
using Content.Shared._Amour.InteractionPanel;
using Robust.Shared.Animations;

namespace Content.Server._Amour.InteractionPanel;

public sealed class Interactions : EntitySystem
{
    [Dependency] private readonly CrawlSystem _crawlSystem = default!;
    [Dependency] private readonly SharebleAnimationSystem _animationSystem = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<InteractionPanelComponent,InteractionBeginningEvent>(OnBegin);
        SubscribeLocalEvent<InteractionPanelComponent,InteractionEndingEvent>(OnEnd);
    }

    private void OnEnd(EntityUid uid, InteractionPanelComponent component, InteractionEndingEvent args)
    {
        if(args.Handled)
            return;

        switch (args.Id)
        {

        }
    }

    private void OnBegin(EntityUid uid, InteractionPanelComponent component, InteractionBeginningEvent args)
    {
        if(args.Handled)
            return;

        switch (args.Id)
        {
            case "PullTarget" :
                _pullingSystem.TryStartPull(uid, args.Target);
                break;
            case "CrawlTarget" :
                _crawlSystem.EnableCrawl(args.Target);
                break;
        }
    }
}
