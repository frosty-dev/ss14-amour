using Content.Shared._White.Cult;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._White.Cult.Items.Components;

[RegisterComponent]
public sealed partial class CultRitesHandComponent : BaseMagicHandComponent
{
    [DataField]
    public SoundSpecifier HealSound = new SoundPathSpecifier("/Audio/White/Cult/rites.ogg");

    [DataField]
    public SoundSpecifier SuckSound = new SoundPathSpecifier("/Audio/White/Cult/enter_blood.ogg");

    [DataField]
    public float HealModifier = 0.5f;

    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<CultistFactoryProductionPrototype>))]
    public List<string> BloodRites = new ()
    {
        "FactoryCultBloodSpear",
        "FactoryCultBloodBarrage"
    };
}
