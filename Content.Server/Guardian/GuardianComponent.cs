using Content.Shared._White.Guardian;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Guardian
{
    /// <summary>
    /// Given to guardians to monitor their link with the host
    /// </summary>
    [RegisterComponent]
    public sealed partial class GuardianComponent : Component
    {
        /// <summary>
        /// The guardian host entity
        /// </summary>
        [DataField]
        public EntityUid? Host;

        /// <summary>
        /// Percentage of damage reflected from the guardian to the host
        /// </summary>
        [DataField("damageShare")]
        public float DamageShare { get; set; } = 0.65f;

        /// <summary>
        /// Maximum distance the guardian can travel before it's forced to recall, use YAML to set
        /// </summary>
        [DataField("distance")]
        public float DistanceAllowed { get; set; } = 5f;

        /// <summary>
        /// Maximum default distance the guardian can travel before it's forced to recall, use YAML to set
        /// </summary>
        [DataField("distanceDefault")]
        public float DistanceAllowedDefault { get; set; } = 10f;

        [DataField]
        public float DistancePowerAssasin { get; set; } = 25f;

        /// <summary>
        /// If the guardian is currently manifested
        /// </summary>
        [DataField]
        public bool GuardianLoose;

        [DataField]
        public GuardianSelector GuardianType = GuardianSelector.Standart;

        [DataField("powerToggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string PowerToggleAction = "ActionGuardianPowerToggle";

        [DataField]
        public EntityUid? PowerToggleActionEntity;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public bool IsInPowerMode;

        [DataField("chargerPowerAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ChargerPowerAction = "ActionChargerPower";

        [DataField]
        public EntityUid? ChargerPowerActionEntity;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public bool IsCharged;

        [ViewVariables(VVAccess.ReadWrite), DataField("assasinDamageModifier")]
        public float AssasinDamageModifier = 3F;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public int LightingCount = 1;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public SoundSpecifier? ChargerSound = new SoundPathSpecifier("/Audio/White/Guardian/charger.ogg");

        [DataField]
        public EntProtoId Action = "ActionToggleGuardian";

        [DataField] public EntityUid? ActionEntity;
    }
}
