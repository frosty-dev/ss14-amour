using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.Animation;

[Serializable,NetSerializable]
public sealed class AnimationProtoStartMessage : EntityEventArgs
{
    public NetEntity Owner;
    public string ProtoId;

    public AnimationProtoStartMessage(NetEntity owner, string protoId)
    {
        Owner = owner;
        ProtoId = protoId;
    }
}

[Serializable,NetSerializable]
public sealed class AnimationStartMessage : EntityEventArgs
{
    public NetEntity Owner;
    public AnimationData Data;
    public string Id;

    public AnimationStartMessage(NetEntity owner, AnimationData data, string id)
    {
        Owner = owner;
        Data = data;
        Id = id;
    }
}
