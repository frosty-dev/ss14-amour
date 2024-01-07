using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.Ryodan.Components;

[RegisterComponent, NetworkedComponent]
public sealed class ThrowingStarAbilityComponent : Component
{
    /// <summary>
    /// The action id.
    /// </summary>
    [DataField("actionId", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string ActionId = string.Empty;

    /// <summary>
    /// Sound played when using action.
    /// </summary>
    [DataField("actionSound"), ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier ActionSound = new SoundPathSpecifier("/Audio/Magic/staff_healing.ogg")
    {
        Params = AudioParams.Default.WithVolume(5f)
    };
}

public sealed class ThrowingStarEvent : InstantActionEvent
{
}
