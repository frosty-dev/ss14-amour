using Robust.Shared.Serialization;

namespace Content.Shared.White.ShittyInteraction.Interactions;

[Serializable,NetSerializable]
public sealed class EbatEvent : BaseInteractionEvent
{
    [DataField("isOchello")] public bool IsOchello;
}


[Serializable,NetSerializable]
public sealed class EbatEndEvent : BaseInteractionEvent
{
}

[Serializable,NetSerializable]
public sealed class CrawledEvent : BaseInteractionEvent
{
}

[Serializable, NetSerializable]
public sealed class ShlepButtEvent : BaseInteractionEvent
{
}

[Serializable, NetSerializable]
public sealed class ShlifovkaEvent : BaseInteractionEvent
{
    [DataField("isPelmashka")] public bool IsPelmashka;
}

[Serializable, NetSerializable]
public sealed class ShlifovkaEndEvent : BaseInteractionEvent
{
}

[Serializable, NetSerializable]
public sealed class ComeToTargetEvent : BaseInteractionEvent
{
    [DataField("kiss")] public bool Kiss;
}

[Serializable, NetSerializable]
public sealed class PullTargetEvent : BaseInteractionEvent
{
}
