using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._White.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class MessagesUiMessageEvent(MessagesUiAction action, string? stringInput, int? targetChatUid) : CartridgeMessageEvent
{
    public readonly MessagesUiAction Action = action;

    public readonly int? TargetChatUid = targetChatUid;

    public readonly string? StringInput = stringInput;
}

[Serializable, NetSerializable]
public enum MessagesUiAction : byte
{
    Send,
    ChangeChat
}
