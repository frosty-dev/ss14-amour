using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._Amour.InteractionPanel.UI;

[GenerateTypedNameReferences]
public sealed partial class InteractionPanelButton : Button
{
    public event Action<string>? OnInteraction;

    private Color _color = Color.White;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            ModulateSelfOverride = value;
        }
    }

    private string _interactionId = "Interaction";
    public string InteractionId
    {
        get => _interactionId;
        set
        {
            _interactionId = value;
            InteractionName.Text = Loc.GetString($"interaction-name-{_interactionId.ToLower()}");
        }
    }

    public InteractionPanelButton()
    {
        RobustXamlLoader.Load(this);
        OnPressed += OnOnPressed;
    }

    private void OnOnPressed(ButtonEventArgs obj)
    {
        OnInteraction?.Invoke(InteractionId);
    }
}

