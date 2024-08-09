using Robust.Shared.Serialization;

namespace Content.Shared._White.CartridgeLoader.Cartridges;


///<summary>
/// Data of a single message in the system, containing the ids of the sender and recipient, the text content and the time it was sent.
///</summary>
[Serializable, NetSerializable]
public struct MessagesMessageData
{
    public int SenderId;
    public int ReceiverId;
    public string Content;
    public TimeSpan Time;
}

[Serializable, NetSerializable]
public sealed class MessagesUserData
{
    public string Name = Loc.GetString("messages-pda-unknown-name");

    public string Job = Loc.GetString("messages-pda-unknown-job");

    public string Department = "Specific";

    public List<MessagesMessageData> Messages = [];

    public void SetMessagesUser(string? name, string? job, string? department)
    {
        Name = name ?? Loc.GetString("messages-pda-unknown-name");
        Job = job ?? Loc.GetString("messages-pda-unknown-job");
        Department = department ?? "Specific";
    }
}
