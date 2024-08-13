using System.Linq;
using Content.Server._White.CartridgeLoader.Cartridges;
using Content.Server._White.Radio.Components;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Station.Systems;
using Content.Shared._White.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;


namespace Content.Server._White.Radio.EntitySystems;

public sealed class MessagesServerSystem : EntitySystem
{
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly SingletonDeviceNetServerSystem _singletonServerSystem = default!;
    [Dependency] private readonly MessagesCartridgeSystem _messagesSystem = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MessagesServerComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<MessagesServerComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(EntityUid uid, MessagesServerComponent component, MapInitEvent args)
    {
        if (!TryComp(uid, out DeviceNetworkComponent? device) || !_singletonServerSystem.SetServerActive(uid, true))
            return;

        _deviceNetworkSystem.ConnectDevice(uid, device);

        var stationIdServer = _stationSystem.GetOwningStation(uid);
        if (!stationIdServer.HasValue)
            return;

        var query = EntityQueryEnumerator<MessagesCartridgeComponent>();

        while (query.MoveNext(out var entityUid, out var cartridge))
        {
            var stationId = _stationSystem.GetOwningStation(entityUid);
            if (stationId.HasValue && stationIdServer == stationId && TryComp(entityUid, out CartridgeComponent? cartComponent))
                _messagesSystem.SendName(entityUid, cartridge, cartComponent, device.Address);
        }
    }

    /// <summary>
    /// Reacts to packets received from clients
    /// </summary>
    private void OnPacketReceived(EntityUid uid, MessagesServerComponent component, DeviceNetworkPacketEvent args)
    {
        if (!_singletonServerSystem.IsActiveServer(uid))
            return;

        if (args.Data.TryGetValue<MessagesUserData>(MessagesNetworkKeys.NewUser, out var messagesUser) && args.Data.TryGetValue<int>(MessagesNetworkKeys.UserId, out var userId))
        {
            component.Dictionary[userId] = messagesUser;

            var packet = new NetworkPayload();
            _deviceNetworkSystem.QueuePacket(uid, args.SenderAddress, packet);
        }

        if (args.Data.TryGetValue<MessagesMessageData>(MessagesNetworkKeys.Message, out var message))
            SendMessage(uid, component, message);
    }

    /// <summary>
    /// Broadcasts a message into the network
    /// </summary>
    private void SendMessage(EntityUid uid, MessagesServerComponent component, MessagesMessageData message)
    {
        component.Dictionary[message.ReceiverId].Messages.Add(message);
        component.Dictionary[message.SenderId].Messages.Add(message);

        _adminLogger.Add(LogType.DeviceNetwork, $"{Loc.GetString("chat-manager-send-message", ("sender", component.Dictionary[message.SenderId].Name + $" ({message.SenderId})"), ("receiver", component.Dictionary[message.ReceiverId].Name + $" ({message.ReceiverId})"), ("message", message.Content))}");
        _chat.SendNetworkChat(uid, Loc.GetString("chat-manager-send-message", ("sender", component.Dictionary[message.SenderId].Name), ("receiver", component.Dictionary[message.ReceiverId].Name), ("message", message.Content)), false);

        var packet = new NetworkPayload()
        {
            [MessagesNetworkKeys.Message] = message
        };

        _deviceNetworkSystem.QueuePacket(uid, null, packet);
    }

    /// <summary>
    /// Returns user
    /// </summary>
    public bool TryGetUserFromDict(EntityUid? uid, int key, out MessagesUserData messagesUserData)
    {
        messagesUserData = new MessagesUserData();

        if (!TryComp(uid, out MessagesServerComponent? component) || !component.Dictionary.TryGetValue(key, out var keyValue))
            return false;

        messagesUserData = keyValue;
        return true;
    }

    /// <summary>
    /// Returns the user dictionary cache
    /// </summary>
    public Dictionary<int, MessagesUserData> GetNameDict(EntityUid? uid)
    {
        return !TryComp(uid, out MessagesServerComponent? component) ? new Dictionary<int, MessagesUserData>() : component.Dictionary;
    }

    /// <summary>
    /// Returns list of messages between the two users
    /// </summary>
    public List<MessagesMessageData> GetMessages(EntityUid? uid, int id1, int id2)
    {
        if (!TryComp(uid, out MessagesServerComponent? component))
            return [];

        return
        [
            ..component.Dictionary[id1]
                .Messages.Where(message =>
                message.SenderId == id1 && message.ReceiverId == id2 ||
                message.SenderId == id2 && message.ReceiverId == id1)
        ];
    }

}

public static class MessagesNetworkKeys
{
    public const string NewUser = "new_user";
    public const string UserId = "user_id";
    public const string Message = "message";
}
