using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.ShittyInteraction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InteractibleComponent : Component
{
    [DataField("isActive"), AutoNetworkedField]
    public bool IsActive;

    [DataField("availableInteractions"), AutoNetworkedField]
    public List<string> AvailableInteractions = new List<string>();

    [DataField("nextInteractiveTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextInteractionTime = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite), DataField("action")]
    public TargetedAction? Action;

    public static string AnimationKey = "interaction_animation";
}
