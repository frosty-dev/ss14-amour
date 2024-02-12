using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Amour.Vibrator;

[RegisterComponent, NetworkedComponent]
public sealed partial class VibratorComponent : Component
{
    [DataField("isVibrating")] public bool IsVibrating = false;

    [DataField("togglePort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string TogglePort = "Toggle";
}


[Serializable, NetSerializable]
public sealed class VibratorComponentState : ComponentState
{
    public bool IsVibrating;

    public VibratorComponentState(bool isVibrating)
    {
        IsVibrating = isVibrating;
    }
}

