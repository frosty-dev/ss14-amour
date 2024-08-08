using Robust.Shared.Serialization;

namespace Content.Shared._White.CartridgeLoader.Cartridges;

///<summary>
/// The state of the messages app interface.
/// Mode switches whether the UI should display a list of other users or a particular chat.
/// Contents contains either the names of users and their ids in the messages system or simply a list of message strings.
///</summary>
[Serializable, NetSerializable]
public sealed class MessagesUiState(MessagesUiStateMode mode, List<(MessagesUserData, int?)>? users = null, List<(string, int?)>? messages = null, string? name = null) : BoundUserInterfaceState
{
    public List<(MessagesUserData, int?)>? Users = users;
    public List<(string, int?)>? Messages = messages;
    public MessagesUiStateMode Mode = mode;
    public string? Name = name;
}

///<summary>
/// Enum representing the modes the program's UI can be in
///</summary>
[Serializable, NetSerializable]
public enum MessagesUiStateMode : byte
{
    UserList,
    Chat,
    Error
}
