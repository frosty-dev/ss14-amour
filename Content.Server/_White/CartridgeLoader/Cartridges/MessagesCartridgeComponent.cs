namespace Content.Server._White.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class MessagesCartridgeComponent : Component
{
    /// <summary>
    /// The component of the last contacted server
    /// </summary>
    [DataField]
    public EntityUid? LastServer;

    /// <summary>
    /// The message system user id of the crew the user is chatting with
    /// </summary>
    [DataField]
    public int? ChatUid;

    [DataField]
    public int? UserUid;
}
