using Robust.Shared.Animations;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.Animation;

[DataDefinition,Serializable,NetSerializable]
public abstract partial class AnimationData
{
    [DataField] public TimeSpan Length = TimeSpan.Zero;
    [DataField] public List<AnimationTrackData> AnimationTracks = new();
}

[DataDefinition,Serializable,NetSerializable]
public sealed partial class Animation : AnimationData
{
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class AnimationTrackData
{
    [DataField] public string ComponentType = "Sprite";
    [DataField] public string Property = "Scale";
    [DataField] public AnimationInterpolationMode InterpolationMode = AnimationInterpolationMode.Previous;
    [DataField] public List<AnimationKeyData> KeyFrames = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class AnimationKeyData
{
    [DataField] public string Value = "0";
    [DataField] public float KeyTime = 0;
}

