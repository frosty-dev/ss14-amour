namespace Content.Shared._Amour.InteractionPanel.Checks;

public interface IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user,
        Entity<InteractionPanelComponent> target, IEntityManager entityManager);
}
