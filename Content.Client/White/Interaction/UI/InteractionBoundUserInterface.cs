using System.Linq;
using Content.Client.White.Interaction.UI.Windows;
using Content.Shared.White.ShittyInteraction;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Interaction.UI;

public sealed class InteractionBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IClientEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly InteractionWindow? _window;

    public InteractionBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _window = new InteractionWindow();
    }

    protected override void Open()
    {
        base.Open();
        var playerEntity = _player.LocalPlayer?.ControlledEntity;
        if (!playerEntity.HasValue)
            return;

        _window?.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Dispose();
    }

    public void SendInteractionMessage(InteractionActionPrototype prototype)
    {
        SendMessage(new InteractionSelectedMessage(prototype.ID));
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);

        if(message is not InteractionAvailableMessage availableMessage)
            return;

        _window?.UpdateInteractions(ToProto(availableMessage.AvailableInteractions),SendInteractionMessage);
    }

    public List<InteractionActionPrototype> ToProto(List<string> inter)
    {
        var protoList = new List<InteractionActionPrototype>();
        foreach (var interaction in inter)
        {
            if(_prototypeManager.TryIndex<InteractionActionPrototype>(interaction,out var prototype))
                protoList.Add(prototype);
        }

        return protoList;
    }
}
