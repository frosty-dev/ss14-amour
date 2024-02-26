namespace Content.Shared._Amour.InteractionPanel.Actions;

public interface IInteractionAction
{
    public void Run(Entity<InteractionPanelComponent> user,
        Entity<InteractionPanelComponent> target, IEntityManager entityManager);
}
