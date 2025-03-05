using Content.Shared.Radio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using Robust.Shared.GameStates;

namespace Content.Shared._White.DeepSpaceCom;

[RegisterComponent, NetworkedComponent]
public sealed partial class DeepSpaceComComponent : Component
{
    [DataField("requiresPower"), ViewVariables(VVAccess.ReadWrite)]
    public bool RequiresPower = true;

    [DataField("supportedChannels", customTypeSerializer: typeof(PrototypeIdListSerializer<RadioChannelPrototype>))]
    public List<string> SupportedChannels = new();
}
