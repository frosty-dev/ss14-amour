using Content.Shared._Amour.Tips;
using Robust.Client.UserInterface;

namespace Content.Client._Amour.Tips;

public sealed class TipsSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _uiMan = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ClippyEvent>(OnClippyEv);
    }

    private void OnClippyEv(ClippyEvent ev)
    {
        _uiMan.GetUIController<ClippyUIController>().AddMessage(ev);
    }
}
