using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Cult.Actions;

[Serializable, NetSerializable]
public sealed partial class ShacklesEvent : DoAfterEvent
{
    public string? Speech;

    public ShacklesEvent(string? speech)
    {
        Speech = speech;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class TwistedConstructionEvent : DoAfterEvent
{
    public NetEntity Effect { get; private set; }

    public EntProtoId RunicDoor { get; private set; }

    public NetCoordinates Location { get; private set; }

    public TwistedConstructionEvent(NetEntity effect, EntProtoId runicDoor, NetCoordinates location)
    {
        Effect = effect;
        RunicDoor = runicDoor;
        Location = location;
    }

    public override DoAfterEvent Clone() => this;
}
