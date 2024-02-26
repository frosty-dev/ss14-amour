using Content.Shared._Amour.Crawl;

namespace Content.Shared._Amour.InteractionPanel.Checks;

public sealed class InteractSelf : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return user == target;
    }
}

public sealed class CantInteractSelf: IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return user != target;
    }
}

public sealed class IsUserCrawl : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.HasComponent<CrawlComponent>(user);
    }
}

public sealed class IsTargetCrawl : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.HasComponent<CrawlComponent>(target);
    }
}
