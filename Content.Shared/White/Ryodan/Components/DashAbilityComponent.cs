using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.Ryodan.Components;

[RegisterComponent, NetworkedComponent]
public sealed class DashAbilityComponent : Component
{
    /// <summary>
    /// The action id for dashing.
    /// </summary>
    [DataField("dashAction", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string DashAction = string.Empty;

    /// <summary>
    /// Sound played when using dash action.
    /// </summary>
    [DataField("blinkSound"), ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg")
    {
        Params = AudioParams.Default.WithVolume(5f)
    };
}

public sealed class DashEvent : WorldTargetActionEvent
{
}
