namespace Content.Shared.DamageableClothing;

/// <summary>
/// This component gets dynamically added to an Entity via the <see cref="DamageableClothing"/>.
/// WD Engi Exclusive.
/// </summary>
[RegisterComponent]
public sealed partial class DamageableClothingUserComponent : Component
{
    /// <summary>
    /// The entity that's also being damaged.
    /// </summary>
    [DataField]
    public EntityUid? ItemId;

}
