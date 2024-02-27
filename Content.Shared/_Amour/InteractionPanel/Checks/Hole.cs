using Content.Shared._Amour.Hole;

namespace Content.Shared._Amour.InteractionPanel.Checks;

public sealed class UserHasButt : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(user.Owner, "Anus");
    }
}

public sealed class TargetHasButt : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(target.Owner, "Anus");
    }
}

public sealed class UserHasPenis : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(user.Owner, "Dick");
    }
}

public sealed class TargetHasPenis : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(target.Owner, "Dick");
    }
}

public sealed class UserHasVagina : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(user.Owner, "Vagina");
    }
}

public sealed class TargetHasVagina : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(target.Owner, "Vagina");
    }
}

public sealed class UserHasTesticles : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(user.Owner, "Testicles");
    }
}

public sealed class TargetHasTesticles : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(target.Owner, "Testicles");
    }
}

public sealed class UserHasBreast : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(user.Owner, "Breast");
    }
}

public sealed class TargetHasBreast : IInteractionCheck
{
    public bool IsAvailable(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        return entityManager.System<SharedHoleSystem>().HasAccessTo(target.Owner, "Breast");
    }
}
