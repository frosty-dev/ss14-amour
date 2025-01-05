using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._White._Engi.Limping;

/// <summary>
/// WD.
/// This is used for the Limping trait.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LimpingComponent : Component
{
    [DataField]
    public float SpeedModifier = 0.3f;

}

[Serializable, NetSerializable]
public sealed class LimpingComponentState : ComponentState
{
    public float SpeedModifier;

    public LimpingComponentState(float speedModifier)
    {
        SpeedModifier = speedModifier;
    }
}
