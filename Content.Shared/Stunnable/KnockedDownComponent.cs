using Content.Shared.Standing.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Stunnable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedStunSystem))]
public sealed partial class KnockedDownComponent : Component
{
    [DataField, AutoNetworkedField]
    public float HelpInterval = 1f;

    [DataField]
    public SoundSpecifier StunAttemptSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

    [ViewVariables, AutoNetworkedField]
    public float HelpTimer = 0f;

    // WD added start
    // Holy shit why is this so long
    [DataField, AutoNetworkedField]
    public SharedStandingStateSystem.DropHeldItemsBehavior KnockDownBehavior = SharedStandingStateSystem.DropHeldItemsBehavior.DropIfStanding;
    // WD added end
}
