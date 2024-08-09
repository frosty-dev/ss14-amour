using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState] // WD
public sealed partial class StaminaDamageOnHitComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("damage")]
    [AutoNetworkedField] // WD
    public float Damage = 30f;

    [DataField("sound")]
    [AutoNetworkedField] // WD
    public SoundSpecifier? Sound;
}
