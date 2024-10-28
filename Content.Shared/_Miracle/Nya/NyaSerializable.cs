using Robust.Shared.Serialization;

namespace Content.Shared._Miracle.Nya;

[Serializable, NetSerializable]
public sealed class ScreengrabResponseEvent : EntityEventArgs
{
    public byte[] Screengrab = new byte[1500000];  // Limit screengrab size to 1.5mbs
}

[Serializable, NetSerializable]
public sealed class ScreengrabRequestEvent : ExpectedReplyEntityEventArgs
{
    [field: NonSerialized]
    public override Type ExpectedReplyType { get; } = typeof(ScreengrabResponseEvent);
}

[Serializable, NetSerializable]
public sealed class CheatCheckResponseEvent : EntityEventArgs
{
    public bool HasPatchMetadata;
    public string? ReflectionOffender;
    public bool HasMoonyware;
    public bool HasHarmony;
    public string? IoCOffender;
    public string? ExtraModuleOffender;
    public string? CvarOffender;
    public string? SystemOffender;
    public string? ComponentOffender;
    public string? WindowOffender;
}

[Serializable, NetSerializable]
public sealed class CheatCheckRequestEvent : ExpectedReplyEntityEventArgs
{
    [field: NonSerialized]
    public override Type ExpectedReplyType { get; } = typeof(CheatCheckResponseEvent);
}

[Serializable, NetSerializable]
public abstract class ExpectedReplyEntityEventArgs : EntityEventArgs
{
    [field: NonSerialized]
    public abstract Type ExpectedReplyType { get; }
}
