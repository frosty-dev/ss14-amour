using Content.Server._White.Radio.Components;
using Content.Server._White.Radio.EntitySystems;
using Content.Server.Administration.Commands;
using Content.Server.CartridgeLoader;
using Content.Shared.CartridgeLoader;
using Content.Shared.PDA;
using Robust.Shared.Map;
using Content.Server.GameTicking;
using Robust.Shared.Timing;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.DeviceNetwork;
using Content.Server.Station.Systems;
using Content.Shared._White.CartridgeLoader.Cartridges;
using Content.Shared.Access.Components;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._White.CartridgeLoader.Cartridges;

public sealed class MessagesCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoaderSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MessagesServerSystem _messagesServerSystem = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly SingletonDeviceNetServerSystem _singletonServerSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MessagesCartridgeComponent, CartridgeMessageEvent>(OnUiMessage);
        SubscribeLocalEvent<MessagesCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<MessagesCartridgeComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        SubscribeLocalEvent<MessagesCartridgeComponent, CartridgeActivatedEvent>(OnCartActivation);
        SubscribeLocalEvent<MessagesCartridgeComponent, CartridgeDeactivatedEvent>(OnCartDeactivation);
        SubscribeLocalEvent<MessagesCartridgeComponent, CartridgeAddedEvent>(OnCartInsertion);
        SubscribeLocalEvent<MessagesCartridgeComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MessagesCartridgeComponent, ComponentRemove>(OnRemove);
    }

    private void OnInit(EntityUid uid, MessagesCartridgeComponent component, ComponentInit args)
    {
        var stationId = _stationSystem.GetOwningStation(uid);
        if (stationId.HasValue &&
            _singletonServerSystem.TryGetActiveServerAddress<MessagesServerComponent>(stationId.Value,
                out var address) && TryComp(uid, out CartridgeComponent? cartComponent))
        {
            SendName(uid, component, cartComponent, address);
            component.UserUid = cartComponent.LoaderUid?.Id;
        }
    }

    private void OnRemove(EntityUid uid, MessagesCartridgeComponent component, ComponentRemove args)
    {
        if (component.LastServer == null || !TryComp<MessagesServerComponent>(component.LastServer, out var messagesServerComponent) || component.UserUid == null)
            return;

        messagesServerComponent.NameDict.Remove(component.UserUid.Value);
    }

    /// <summary>
    /// This gets called when the ui fragment needs to be updated for the first time after activating
    /// </summary>
    private void OnUiReady(EntityUid uid, MessagesCartridgeComponent component, CartridgeUiReadyEvent args)
    {
        var stationId = _stationSystem.GetOwningStation(uid);
        if (stationId.HasValue && _singletonServerSystem.TryGetActiveServerAddress<MessagesServerComponent>(stationId.Value, out var address) && TryComp(uid, out CartridgeComponent? cartComponent))
            SendName(uid, component, cartComponent, address);
        UpdateUiState(uid, component);
    }

    /// <summary>
    /// The ui messages received here get wrapped by a CartridgeMessageEvent and are relayed from the <see cref="CartridgeLoaderSystem"/>
    /// </summary>
    /// <remarks>
    /// The cartridge specific ui message event needs to inherit from the CartridgeMessageEvent
    /// </remarks>
    private void OnUiMessage(EntityUid uid, MessagesCartridgeComponent component, CartridgeMessageEvent args)
    {
        if (args is not MessagesUiMessageEvent messageEvent)
            return;

        if (messageEvent.Action == MessagesUiAction.Send && TryComp(uid, out CartridgeComponent? cartComponent) && component.UserUid is { } userId && component.ChatUid != null && messageEvent.StringInput != null)
        {
            var stationId = _stationSystem.GetOwningStation(uid);
            if (!stationId.HasValue)
                return;
            MessagesMessageData messageData = new()
            {
                SenderId = userId,
                ReceiverId = component.ChatUid.Value,
                Content = messageEvent.StringInput,
                Time = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan)
            };
            var packet = new NetworkPayload()
            {
                [MessagesNetworkKeys.Message] = messageData
            };
            _singletonServerSystem.TryGetActiveServerAddress<MessagesServerComponent>(stationId.Value, out var address);
            _deviceNetworkSystem.QueuePacket(uid, address, packet);
        }
        else
        {
            if (messageEvent.Action == MessagesUiAction.ChangeChat)
                component.ChatUid = messageEvent.TargetChatUid;
        }

        UpdateUiState(uid, component);
    }

    /// <summary>
    /// On cart insertion, register as background process.
    /// </summary>
    private void OnCartInsertion(EntityUid uid, MessagesCartridgeComponent component, CartridgeAddedEvent args)
    {
        _cartridgeLoaderSystem.RegisterBackgroundProgram(args.Loader, uid);
        component.UserUid = args.Loader.Id;
    }

    /// <summary>
    /// On cartridge activation, connect to messages network.
    /// </summary>
    private void OnCartActivation(EntityUid uid, MessagesCartridgeComponent component, CartridgeActivatedEvent args)
    {
        _deviceNetworkSystem.ConnectDevice(uid);
        var stationId = _stationSystem.GetOwningStation(uid);
        if (stationId.HasValue && _singletonServerSystem.TryGetActiveServerAddress<MessagesServerComponent>(stationId.Value, out var address) && TryComp(uid, out CartridgeComponent? cartComponent))
            SendName(uid, component, cartComponent, address);
    }

    /// <summary>
    /// On cartridge deactivation, disconnect from messages network.
    /// </summary>
    private void OnCartDeactivation(EntityUid uid, MessagesCartridgeComponent component, CartridgeDeactivatedEvent args)
    {
        _deviceNetworkSystem.DisconnectDevice(uid, null);
    }

    /// <summary>
    /// React and respond to packets from the server
    /// </summary>
    private void OnPacketReceived(EntityUid uid, MessagesCartridgeComponent component, DeviceNetworkPacketEvent args)
    {
        if (!TryComp(uid, out CartridgeComponent? cartComponent))
            return;
        component.LastServer = args.Sender;
        if (args.Data.TryGetValue<MessagesMessageData>(MessagesNetworkKeys.Message, out var message) && cartComponent.LoaderUid != null)
        {
            if (message.ReceiverId == cartComponent.LoaderUid.Value.Id)
            {
                TryGetName(message.SenderId, component, out var name);

                var subtitleString = Loc.GetString("messages-pda-notification-header", ("name", name));

                _cartridgeLoaderSystem.SendNotification(
                    cartComponent.LoaderUid.Value,
                    subtitleString,
                    message.Content);
            }
        }

        UpdateUiState(uid, component);

    }

    /// <summary>
    /// Updates the user's name in the storage component.
    /// </summary>
    private void SendName(EntityUid uid, MessagesCartridgeComponent component, CartridgeComponent cartComponent, string? address)
    {
        TryGetMessagesUser(cartComponent, out var messagesUser);

        var packet = new NetworkPayload()
        {
            [MessagesNetworkKeys.UserId] = component.UserUid,
            [MessagesNetworkKeys.NewUser] = messagesUser
        };
        _deviceNetworkSystem.QueuePacket(uid, address, packet);
    }

    /// <summary>
    /// Retrieves the name of the given user from the last contacted server
    /// </summary>
    private bool TryGetName(int key, MessagesCartridgeComponent component, out string name)
    {
        if (component.LastServer != null && _messagesServerSystem.TryGetUserFromDict(component.LastServer, key, out var messagesUser))
        {
            name = messagesUser.Name;
            return true;
        }

        name = Loc.GetString("messages-pda-connection-error");
        return false;
    }

    /// <summary>
    /// Returns the user's name, job title and job department
    /// </summary>
    public bool TryGetMessagesUser(CartridgeComponent component, out MessagesUser messagesUser)
    {
        var pda = component.LoaderUid;
        if (pda == null)
        {
            messagesUser = new MessagesUser(Loc.GetString("messages-pda-unknown-name"), Loc.GetString("messages-pda-unknown-job"), "Specific");
            return false;
        }

        var pdaComponent = CompOrNull<PdaComponent>(pda);
        if (pdaComponent?.OwnerName == null)
        {
            messagesUser = new MessagesUser(Loc.GetString("messages-pda-unknown-name"), Loc.GetString("messages-pda-unknown-job"), "Specific");
            return false;
        }

        messagesUser = new MessagesUser(pdaComponent.OwnerName, pdaComponent.OwnerJob ?? Loc.GetString("messages-pda-unknown-job"), pdaComponent.OwnerDepartment ?? "Specific");
        return true;
    }

    private void UpdateUiState(EntityUid uid, MessagesCartridgeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        if (!TryComp(uid, out CartridgeComponent? cartComponent))
            return;
        if (cartComponent.LoaderUid == null)
            return;
        var loaderUid = cartComponent.LoaderUid.Value;
        MessagesUiState state;
        MapId mapId = Transform(uid).MapID;
        int? currentUserId = component.UserUid;
        if (currentUserId == null || component.LastServer == null)
        {
            state = new MessagesUiState(MessagesUiStateMode.Error, [], null);
            _cartridgeLoaderSystem.UpdateCartridgeUiState(loaderUid, state);
            return;
        }
        if (component.ChatUid == null) //if no chat is loaded, list users
        {
            List<(MessagesUser, int?)> userList = [];

            var nameDict = _messagesServerSystem.GetNameDict(component.LastServer);

            foreach (var nameEntry in nameDict.Keys)
            {
                if (nameEntry == currentUserId)
                    continue;
                userList.Add((nameDict[nameEntry], nameEntry));
            }

            state = new MessagesUiState(MessagesUiStateMode.UserList, userList, null);
        }
        else
        {
            List<MessagesMessageData> messageList = []; //Else, list messages from the current chat

            foreach (var message in _messagesServerSystem.GetMessages(component.LastServer, component.ChatUid.Value, currentUserId.Value))
            {
                if (message.SenderId == component.ChatUid && message.ReceiverId == currentUserId || message.ReceiverId == component.ChatUid && message.SenderId == currentUserId)
                {
                    messageList.Add(message);
                }
            }

            messageList.Sort
            (
                delegate (MessagesMessageData a, MessagesMessageData b)
                {
                    return TimeSpan.Compare(a.Time, b.Time);
                }
            );

            List<(MessagesUser, int?)> formattedMessageList = [];

            foreach (var message in messageList)
            {
                TryGetName(message.SenderId, component, out var name);
                var stationTime = message.Time.Subtract(_gameTicker.RoundStartTimeSpan);
                var content = $"{stationTime.ToString("\\[hh\\:mm\\:ss\\]")} {name}: {message.Content}";
                formattedMessageList.Add((new MessagesUser(content, Loc.GetString("messages-pda-unknown-job"), "Specific"), null));
            }

            TryGetName(component.ChatUid.Value, component, out var user);
            state = new MessagesUiState(MessagesUiStateMode.Chat, formattedMessageList, user);
        }
        _cartridgeLoaderSystem.UpdateCartridgeUiState(loaderUid, state);
    }
}
