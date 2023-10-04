using Robust.Shared.Serialization;

namespace Content.Shared.White.ShittyInteraction.Interactions;

[Serializable,NetSerializable]
public sealed class EbatEvent : BaseInteractionEvent
{
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
}
