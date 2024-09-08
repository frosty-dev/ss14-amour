namespace Content.Shared.Wieldable;

/// <summary>
/// Raised directed on an entity when it is wielded.
/// </summary>
// WD edit start
// Reason for the edit: previously ItemWieldedEvent didn't contained "EntityUid user" parameter.
// Now it's done like ItemUnwieldedEvent with "EntityUid user" parameter for correct logic work.
// [ByRefEvent]
// public readonly record struct ItemWieldedEvent;
public sealed class ItemWieldedEvent : EntityEventArgs
{
    public EntityUid? User;
    /// <summary>
    ///     Whether the item is being forced to be wielded, or if the player chose to wield it themselves.
    /// </summary>
    public bool Force;

    public ItemWieldedEvent(EntityUid? user = null, bool force = false)
    {
        User = user;
        Force = force;
    }
}
// WD edit end
