namespace Content.Shared._White._Engi.DamageableClothing;

/// <summary>
/// WD.
/// This component gets dynamically added to an Entity via the <see cref="DamageableClothing"/>.
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
