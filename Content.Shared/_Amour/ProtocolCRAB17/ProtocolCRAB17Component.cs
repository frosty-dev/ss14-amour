using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.ProtocolCRAB17;

/// <summary>
/// WD
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class ProtocolCRAB17Component : Component
{
    [ViewVariables]
    public int? BankAccountId;

    [ViewVariables]
    public bool HasBankAccountId = false;

    [DataField]
    public SoundSpecifier UseSound = new SoundPathSpecifier("/Audio/_Amour/ProtocolCRAB17/Phone_Dial.ogg");

    [DataField]
    public SoundSpecifier SoundApply = new SoundPathSpecifier("/Audio/White/Machines/chime.ogg");

}

[Serializable, NetSerializable]
public sealed partial class ProtocolCRAB17DoAfterEvent : SimpleDoAfterEvent
{
}
