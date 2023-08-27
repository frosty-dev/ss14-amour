using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.White.Dildo;

[RegisterComponent, NetworkedComponent]
public sealed class DildoComponent : Component
{
    [DataField("isVibrating")] public bool IsVibrating = false;

    [DataField("togglePort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string TogglePort = "Toggle";
}


[Serializable, NetSerializable]
public sealed class DildoComponentState : ComponentState
{
    public bool IsVibrating;

    public DildoComponentState(bool isVibrating)
    {
        IsVibrating = isVibrating;
    }
}
