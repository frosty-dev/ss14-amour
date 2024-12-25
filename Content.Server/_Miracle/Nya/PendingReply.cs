using Content.Shared._Miracle.Nya;

namespace Content.Server._Miracle.Nya;

public sealed class PendingReply(
    ExpectedReplyEntityEventArgs request,
    TimeSpan timeout,
    Action<EntityEventArgs, EntitySessionEventArgs> handler)
{
    public ExpectedReplyEntityEventArgs Request { get; } = request;
    public TimeSpan TimeoutTime { get; } = timeout;
    public Action<EntityEventArgs, EntitySessionEventArgs> Handler { get; } = handler;
}
