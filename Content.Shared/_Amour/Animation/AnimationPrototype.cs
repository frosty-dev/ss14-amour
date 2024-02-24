using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.Animation;

[Prototype("animation"),Serializable,NetSerializable]
public sealed partial class AnimationPrototype : AnimationData, IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
}
