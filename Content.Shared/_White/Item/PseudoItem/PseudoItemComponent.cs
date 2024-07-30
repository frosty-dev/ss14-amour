using Content.Shared.Item;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Item.PseudoItem;

/// <summary>
/// For entities that behave like an item under certain conditions,
/// but not under most conditions.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PseudoItemComponent : Component
{
    [DataField]
    public ProtoId<ItemSizePrototype> Size = "Huge";

    [DataField]
    public List<Box2i>? Shape = new() {new Box2i(0, 0, 4, 4)};

    [AutoNetworkedField]
    public bool Active;
}
