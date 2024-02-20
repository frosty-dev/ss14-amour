using Content.Server.EUI;

namespace Content.Server._Amour.InteractionPanel;

public sealed class InteractionPanelEui : BaseEui
{
    public InteractionPanelEui()
    {
        IoCManager.InjectDependencies(this);
    }
}
