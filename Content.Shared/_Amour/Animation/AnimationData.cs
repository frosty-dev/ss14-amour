using Robust.Shared.Animations;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.Animation;

[DataDefinition,Serializable,NetSerializable,Virtual]
public partial class AnimationData
{
    [DataField] public TimeSpan Length;
    [DataField] public List<AnimationTrackData> AnimationTracks;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class AnimationTrackData
{
    [DataField] public string ComponentType;
    [DataField] public string Property;
    [DataField] public AnimationInterpolationMode InterpolationMode;
    [DataField] public List<AnimationKeyData> KeyFrames;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class AnimationKeyData
{
    [DataField] public string Value;
    [DataField] public float KeyTime;
}

