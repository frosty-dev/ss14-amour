using Content.Server.Chat.Managers;
using Robust.Shared.Player;

namespace Content.Server._White.AspectsSystem.Base;

public sealed class ChatHelper : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    public void SendAspectDescription(EntityUid mob, string messageKey)
    {
        _playerManager.TryGetSessionByEntity(mob, out var session);
        if (session != null)
        {
            _chatManager.DispatchServerMessage(session, Loc.GetString(messageKey));
        }
    }
}
