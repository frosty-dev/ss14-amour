using Content.Client.Actions;
using Content.Client.Gameplay;
using Content.Client.Interactable;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Emotions;
using Content.Client.UserInterface.Systems.Interaction.Windows;
using Content.Client.White.Interaction;
using Content.Shared.Input;
using Content.Shared.White.ShittyInteraction;
using Robust.Client;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.Timing;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client.UserInterface.Systems.Interaction;

public sealed class InteractionUiController : UIController, IOnStateChanged<GameplayState>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IClientEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    [UISystemDependency] private readonly InteractibleSystem _interaction = default!;
    [UISystemDependency] private readonly ActionsSystem _actionsSystem = default!;

    private InteractionWindow? _window;
    private MenuButton? InteractionButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.InteractionButton;
    public void OnStateEntered(GameplayState state)
    {
        _window = new InteractionWindow();
        // _interaction = _entity.System<InteractibleSystem>();

        _window.OnOpen += OnWindowOpened;
        _window.OnClose += OnWindowClosed;

        UpdateInteractionList();

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenInteractionMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<InteractionUiController>();
    }

    private void OnWindowClosed()
    {
        if (InteractionButton != null)
            InteractionButton.Pressed = false;
    }

    private void OnWindowOpened()
    {
        if (InteractionButton != null)
            InteractionButton.Pressed = true;
    }

    public void LoadButton()
    {
        if (InteractionButton != null)
            InteractionButton.OnPressed += InteractionButtonPressed;
    }

    public void UnloadButton()
    {
        if (InteractionButton != null)
            InteractionButton.OnPressed -= InteractionButtonPressed;
    }

    private void InteractionButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        ToggleWindow();
    }

    public void UpdateInteractionList()
    {
        var playerEntity = _player.LocalPlayer?.ControlledEntity;
        if (!playerEntity.HasValue)
            return;

        _window?.InteractionsContainer.Children.Clear();
        foreach (var interaction in _interaction.TryGetAvailableInteractions(playerEntity.Value))
        {
            if (_prototypeManager.TryIndex<InteractionActionPrototype>(interaction, out var action))
            {
                var control = new Button();
                control.OnPressed += _ =>
                    _entity.EntityNetManager?.SendSystemNetworkMessage(new InteractionSelectMessage(interaction));
                control.Text = Loc.GetString(action.DisplayName);
                control.HorizontalExpand = true;
                control.VerticalExpand = true;
                control.MaxWidth = 250;
                control.MaxHeight = 30;
                _window?.InteractionsContainer.AddChild(control);
            }
        }
    }

    private void ToggleWindow()
    {
        if (_window == null)
            return;

        if (_window.IsOpen)
        {
            _window.Close();
            return;
        }

        _window.Open();
        UpdateInteractionList();
    }

    public void SelectIntegration(string interaction)
    {
        _entity.EntityNetManager?.SendSystemNetworkMessage(new InteractionSelectMessage(interaction));
        ToggleWindow();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
            _window.OnOpen -= OnWindowOpened;
            _window.OnClose -= OnWindowClosed;

            _window.Dispose();
            _window = null;
        }

        CommandBinds.Unregister<InteractionUiController>();
    }
}
