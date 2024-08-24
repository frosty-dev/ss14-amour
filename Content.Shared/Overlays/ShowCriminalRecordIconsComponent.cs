using Content.Shared.Radio;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Overlays;

/// <summary>
///     This component allows you to see criminal record status of mobs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShowCriminalRecordIconsComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<StatusIconPrototype>))]
    public IReadOnlyCollection<string> Status = ArraySegment<string>.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<RadioChannelPrototype> SecurityChannel = "Security";

}
