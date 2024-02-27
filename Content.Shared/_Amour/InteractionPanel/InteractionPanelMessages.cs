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
public sealed class InteractionUpdateMessage : EuiMessageBase
{
}

[Serializable,NetSerializable]
public sealed class InteractionState: EuiStateBase
{
    public NetEntity Performer { get; }
    public NetEntity Target { get; }
    public HashSet<InteractionEntry> AvailableInteractions;

    public HashSet<string> DescUser;
    public HashSet<string> DescTarget;

    public byte? Arousal;

    public InteractionState(NetEntity performer, NetEntity target, HashSet<InteractionEntry> availableInteractions, byte? arousal, HashSet<string> descUser, HashSet<string> descTarget)
    {
        Performer = performer;
        Target = target;
        AvailableInteractions = availableInteractions;
        Arousal = arousal;
        DescUser = descUser;
        DescTarget = descTarget;
    }
}

[Serializable, NetSerializable]
public sealed class InteractionEntry
{
    public string Prototype;
    public bool Enabled;

    public InteractionEntry(string prototype, bool enabled)
    {
        Prototype = prototype;
        Enabled = enabled;
    }
}
