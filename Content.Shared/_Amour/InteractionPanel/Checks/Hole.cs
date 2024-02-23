using Content.Shared._Amour.Hole;

namespace Content.Shared._Amour.InteractionPanel.Checks;

public sealed class UserHasButt : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(user.Owner, "Anus", out _);
    }
}

public sealed class TargetHasButt : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(target.Owner, "Anus", out _);
    }
}

public sealed class UserHasPenis : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(user.Owner, "Dick", out _);
    }
}

public sealed class TargetHasPenis : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(target.Owner, "Dick", out _);
    }
}

public sealed class UserHasVagina : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(user.Owner, "Vagina", out _);
    }
}

public sealed class TargetHasVagina : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(target.Owner, "Vagina", out _);
    }
}

public sealed class UserHasTesticles : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(user.Owner, "Testicles", out _);
    }
}

public sealed class TargetHasTesticles : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(target.Owner, "Testicles", out _);
    }
}

public sealed class UserHasBreast : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(user.Owner, "Breast", out _);
    }
}

public sealed class TargetHasBreast : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().TryFind(target.Owner, "Breast", out _);
    }
}
