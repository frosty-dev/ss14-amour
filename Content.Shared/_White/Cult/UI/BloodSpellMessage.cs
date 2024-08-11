using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Cult.UI;

public enum BloodSpellMessageState : byte
{
    Create,
    Remove,
    Cancel
}

[Serializable, NetSerializable]
public sealed class BloodSpellMessage : EuiMessageBase
{
    public readonly BloodSpellMessageState State;

    public BloodSpellMessage(BloodSpellMessageState state)
    {
        State = state;
    }
}
