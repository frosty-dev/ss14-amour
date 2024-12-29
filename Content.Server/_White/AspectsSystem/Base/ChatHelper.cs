using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Robust.Shared.Player;

namespace Content.Server._White.AspectsSystem.Base
{
    public static class ChatHelper
    {
        [Dependency] private static ISharedPlayerManager _playerManager = default!;
        [Dependency] private static IChatManager _chatManager = default!;




        public static void Initialize(ISharedPlayerManager playerManager, IChatManager chatManager)
        {
            _playerManager = playerManager;
            _chatManager = chatManager;
        }
        public static void SendAspectDescription(EntityUid mob, string messageKey)
        {
            _playerManager.TryGetSessionByEntity(mob, out var session);
            if (session != null)
            {
                _chatManager.DispatchServerMessage(session, Loc.GetString(messageKey));
            }
        }
    }
}
