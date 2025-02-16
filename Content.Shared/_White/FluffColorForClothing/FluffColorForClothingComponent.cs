using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.FluffColorForClothing;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
public sealed partial class FluffColorForClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId Action = "ActionFluffColorForClothing";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    [AutoNetworkedField]
    public string CurrentColor = "white";

    [DataField]
    public List<string> Colors = new() { "white" };

    [DataField]
    public string VerbText = "Поменять цвет";

    [DataField]
    public string Specifier = "default";

    [DataField]
    public bool MainItem = false;

    [DataField]
    public EntityUid? User;

    [DataField("requiredSlot"), AutoNetworkedField]
    public SlotFlags RequiredFlags = SlotFlags.NECK;

}
