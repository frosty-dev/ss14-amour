using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.White.ShockClothing;

[RegisterComponent]
public sealed class ShockClothingComponent : Component
{
    [DataField("triggerPort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string TriggerPort = "Trigger";

    [ViewVariables(VVAccess.ReadWrite), DataField("zapSound")]
    public SoundSpecifier? ZapSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/taser_hit.ogg");
}
