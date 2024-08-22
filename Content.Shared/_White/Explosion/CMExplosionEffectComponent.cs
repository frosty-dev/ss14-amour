using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Explosion;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedExplosionSystem))]
public sealed partial class ExplosionEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId Explosion = "ExplosionEffectGrenade";

    [DataField, AutoNetworkedField]
    public EntProtoId ShockWave = "ExplosionEffectGrenadeShockWave";

    [DataField, AutoNetworkedField]
    public List<EntProtoId> ShrapnelEffects = new() { "ExplosionEffectShrapnel1", "ExplosionEffectShrapnel2" };

    [DataField, AutoNetworkedField]
    public int MinShrapnel = 5;

    [DataField, AutoNetworkedField]
    public int MaxShrapnel = 9;

    [DataField, AutoNetworkedField]
    public float ShrapnelSpeed = 5;
}
