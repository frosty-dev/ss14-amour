using Content.Shared.Interaction;

namespace Content.Shared._Amour.InteractionPanel.Checks;

public sealed class HasSmallDistance : IInteractionCheck
{
    [DataField] private readonly float _range = SharedInteractionSystem.InteractionRange;
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        var transformSystem = entityManager.System<SharedTransformSystem>();

        if (!entityManager.HasComponent<TransformComponent>(user) ||
            !entityManager.HasComponent<TransformComponent>(target))
            return false;

        if (_range <= 0)
            return true;

        var distance = (transformSystem.GetWorldPosition(user) - transformSystem.GetWorldPosition(target)).Length();
        return distance <= _range;
    }
}
