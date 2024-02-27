using Content.Shared.Hands.Components;

namespace Content.Shared._Amour.InteractionPanel.Checks;

public sealed class HasItemInUserHand : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.TryGetComponent<HandsComponent>(user, out var handsComponent) && handsComponent.ActiveHand?.HeldEntity is not null;
    }
}

public sealed class UserHasHand : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.HasComponent<HandsComponent>(user);
    }
}

public sealed class TargetHasHand : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.HasComponent<HandsComponent>(target);
    }
}
