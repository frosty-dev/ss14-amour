using Content.Server._White.AspectsSystem.Base;
using Content.Server.Chat.Managers;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Server._White;

public static class UnsafePseudoIoC // Я НАНАВИЖУ IOCMAANGERRESOLVEPOSHEL NAHUI
{
    public static IConfigurationManager ConfigurationManager = default!;

    public static void Initialize()
    {
        ConfigurationManager = IoCManager.Resolve<IConfigurationManager>();
        ChatHelper.Initialize(IoCManager.Resolve<ISharedPlayerManager>(), IoCManager.Resolve<IChatManager>());// heh IOCMAANGERRESOLVEPOSHEL NAHUI X2
    }

}
