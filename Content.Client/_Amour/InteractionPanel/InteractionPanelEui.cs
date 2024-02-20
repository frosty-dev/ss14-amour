using Content.Client.Eui;
using Content.Shared.Eui;

namespace Content.Client._Amour.InteractionPanel;

public sealed class InteractionPanelEui : BaseEui
{
    private InteractionPanelWindow _interactionPanelWindow;

    public InteractionPanelEui()
    {
        _interactionPanelWindow = new InteractionPanelWindow();
        _interactionPanelWindow.OnClose += () => SendMessage(new CloseEuiMessage());
    }

    public override void Closed()
    {
        base.Closed();
        _interactionPanelWindow.Close();
    }

    public override void Opened()
    {
        base.Opened();
        _interactionPanelWindow.OpenCentered();
    }
}
