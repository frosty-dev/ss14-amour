using Content.Shared._White.DeepSpaceCom;
using JetBrains.Annotations;

namespace Content.Client._White.DeepSpaceCom;

[UsedImplicitly]
public sealed class DeepSpaceComBoundUI : BoundUserInterface
{
    [ViewVariables]
    private DeepSpaceComMenu? _menu;

    public DeepSpaceComBoundUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        _menu = new();

        _menu.OnMicPressed += enabled =>
        {
            SendMessage(new ToggleDeepSpaceComMicrophoneMessage(enabled));
        };
        _menu.OnSpeakerPressed += enabled =>
        {
            SendMessage(new ToggleDeepSpaceComSpeakerMessage(enabled));
        };
        _menu.OnChannelSelected += channel =>
        {
            SendMessage(new SelectDeepSpaceComChannelMessage(channel));
        };

        _menu.OnClose += Close;
        _menu.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Close();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not DeepSpaceComBoundUIState msg)
            return;

        _menu?.Update(msg);
    }
}
