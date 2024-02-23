using Content.Shared.Eui;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.InteractionPanel;

[Serializable,NetSerializable]
public sealed class InteractionSelectedMessage : EuiMessageBase
{
    public string Id;

    public InteractionSelectedMessage(string id)
    {
        Id = id;
    }
}


[Serializable,NetSerializable]
public sealed class InteractionState: EuiStateBase
{
    public NetEntity Performer { get; }
    public NetEntity Target { get; }
    public HashSet<string> AvailableInteractions;
    public byte? Arousal;

    public InteractionState(NetEntity performer, NetEntity target, HashSet<string> availableInteractions, byte? arousal)
    {
        Performer = performer;
        Target = target;
        AvailableInteractions = availableInteractions;
        Arousal = arousal;
    }
}

