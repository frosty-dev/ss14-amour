using Content.Shared.Damage;
using Robust.Shared.GameStates;
// WD Engi Exclusive
namespace Content.Shared.DamageableClothing;

/// <summary>
/// This component goes on an equippable item that should take damage while in use.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DamageableClothingComponent : Component
{

    /// <summary>
    /// The entity that's wearing the item.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? User;

    /// <summary>
    /// The damage modifier to use on item.
    /// </summary>
    [DataField]
    public DamageModifierSet DamageModifier = default!;

}
