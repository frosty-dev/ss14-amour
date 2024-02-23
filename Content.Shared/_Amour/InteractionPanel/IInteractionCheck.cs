using Content.Shared._Amour.Hole;

namespace Content.Shared._Amour.InteractionPanel;

public interface IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user,
        Entity<InteractionPanelComponent> target,IEntityManager entityManager);
}

public sealed class BasicCheck : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        Logger.Debug("MEWO!!");
        return true;
    }
}
