using Content.Client.Eui;
using Content.Shared._Amour.InteractionPanel;
using Content.Shared.Eui;

namespace Content.Client._Amour.InteractionPanel;

public sealed class InteractionPanelEui : BaseEui
{
    private readonly IEntityManager _entityManager;

    private UI.InteractionPanelWindow _interactionPanelWindow;
    private InteractionState _interactionState = default!;

    public InteractionPanelEui()
    {
        IoCManager.InjectDependencies(this);
        _entityManager = IoCManager.Resolve<IEntityManager>();

        _interactionPanelWindow = new UI.InteractionPanelWindow();
        _interactionPanelWindow.OnClose += () => SendMessage(new CloseEuiMessage());
        _interactionPanelWindow.OnInteraction += InteractionPanelWindowOnInteraction;
        _interactionPanelWindow.OnUpdateRequired += () => SendMessage(new InteractionUpdateMessage());
    }

    private void InteractionPanelWindowOnInteraction(string id)
    {
        SendMessage(new InteractionSelectedMessage(id));
    }

    public override void HandleState(EuiStateBase state)
    {
        base.HandleState(state);

        if(state is not InteractionState interactionState)
            return;

        _interactionState = interactionState;
        _interactionPanelWindow.Update(_interactionState);
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
