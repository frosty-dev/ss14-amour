using Content.Client.UserInterface.Fragments;
using Content.Shared._White.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;

namespace Content.Client._White.CartridgeLoader.Cartridges;

public sealed partial class MessagesUi : UIFragment
{
    private MessagesUiFragment _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new MessagesUiFragment();
        _fragment.OnMessageSent += note => SendMessagesMessage(MessagesUiAction.Send, note, null, userInterface);
        _fragment.OnButtonPressed += userUid => SendMessagesMessage(MessagesUiAction.ChangeChat, null, userUid, userInterface);
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MessagesUiState messagesState)
            return;

        _fragment.UpdateState(messagesState.Mode, messagesState.Contents, messagesState.Name);
    }

    private void SendMessagesMessage(MessagesUiAction action, string? stringInput, int? uidInput, BoundUserInterface userInterface)
    {
        var messagesMessage = new MessagesUiMessageEvent(action, stringInput, uidInput);
        var message = new CartridgeUiMessage(messagesMessage);
        userInterface.SendMessage(message);
    }
}
