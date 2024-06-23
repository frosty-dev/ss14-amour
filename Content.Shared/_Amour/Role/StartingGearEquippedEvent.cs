namespace Content.Shared._Amour.Role;

/// <summary>
/// Raised directed on an entity when a new starting gear prototype has been equipped.
/// </summary>
[ByRefEvent]
public record struct StartingGearEquippedEvent(EntityUid Entity)
{
    public readonly EntityUid Entity = Entity;
}
