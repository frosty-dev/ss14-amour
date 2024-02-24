using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Value;

namespace Content.Shared._Amour.Animation;

public abstract class SharedAnimationSystem : EntitySystem
{
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    public virtual void Play(EntityUid uid,ProtoId<AnimationPrototype> protoId)
    {
    }

    public virtual void Play(EntityUid uid,AnimationData data, string animationId = "funny")
    {
    }

    public AnimationKeyData KeyFrame(object value, float keyTime)
    {
        return new AnimationKeyData()
        {
            KeyTime = keyTime,
            Value = _serializationManager.WriteValueAs<ValueDataNode>(value).Value
        };
    }
}
