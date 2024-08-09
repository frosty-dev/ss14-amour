using Content.Server.Power.Components;
using Content.Shared._White.CartridgeLoader.Cartridges;


namespace Content.Server._White.Radio.Components;

/// <summary>
/// Entities with <see cref="MessagesServerComponent"/> are needed to transmit messages using PDAs.
/// They also need to be powered by <see cref="ApcPowerReceiverComponent"/>
/// in order for them to work on the same map as server.
/// </summary>
[RegisterComponent]
public sealed partial class MessagesServerComponent : Component
{
    /// <summary>
    /// Dictionary translating IDs to MessagesUser
    /// </summary>
    [DataField]
    public Dictionary<int, MessagesUserData> Dictionary = new();
}
