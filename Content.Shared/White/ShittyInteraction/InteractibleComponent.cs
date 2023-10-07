using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.ShittyInteraction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InteractibleComponent : Component
{
    [DataField("isActive"), AutoNetworkedField]
    public bool IsActive;

    [DataField("nextInteractiveTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextInteractionTime = TimeSpan.Zero;

    public static string AnimationKey = "interaction_animation";
}

[NetSerializable, Serializable]
public enum InteractionUiKey : byte
{
    Key,
}
