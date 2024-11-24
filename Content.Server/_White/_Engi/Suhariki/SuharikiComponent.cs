using Robust.Shared.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Audio;

namespace Content.Server._White._Engi.Suhariki;

/// <summary>
/// WD.
/// Makes you loose your tooth and have funny accent.
/// </summary>
[RegisterComponent, Access(typeof(SuharikiSystem))]
public sealed partial class SuharikiComponent : Component

{
    /// <summary>
    /// Amount and type of damage that will be dealt on event.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Chance of event activation.
    /// </summary>
    [DataField]
    public float Chance = 0;

    /// <summary>
    /// Amount of rolls for event.
    /// </summary>
    [DataField]
    public int StonesInFood = 1;

    /// <summary>
    /// The prototype that will be spawned on event.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string HoldingPrototype = "SuharikiTooth";

    /// <summary>
    /// Emote triggered on event.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EmotePrototype>))]
    public string EmoteId = "Scream";

    /// <summary>
    /// Sound triggered on event.
    /// </summary>
    [DataField]
    public SoundSpecifier UseSound { get; set; } = new SoundPathSpecifier("/Audio/White/_Engi/Object/Misc/Suhariki/tooth_break.ogg");
}
