using Content.Shared._White.Guardian;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Guardian
{
    /// <summary>
    /// Creates a GuardianComponent attached to the user's GuardianHost.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class GuardianCreatorComponent : Component
    {
        /// <summary>
        /// Counts as spent upon exhausting the injection
        /// </summary>
        /// <remarks>
        /// We don't mark as deleted as examine depends on this.
        /// </remarks>
        [DataField]
        public bool Used = false;

        /// <summary>
        /// The prototype of the guardian entity which will be created
        /// </summary>
        [DataField("guardianProto",
            customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>),
            required: true)]
        public string GuardianProto;

        /// <summary>
        /// How long it takes to inject someone.
        /// </summary>
        [DataField("delay")]
        public float InjectionDelay = 5f;

        [DataField("guardiansAvaliable")]
        public IReadOnlyCollection<string> GuardiansAvaliable = ArraySegment<string>.Empty;

        [DataField]
        public Dictionary<GuardianSelector, string> GuardianSelectorToProto = new()
        {
            { GuardianSelector.Assasin, "MobHoloparasiteGuardianAssasin" },
            { GuardianSelector.Standart, "MobHoloparasiteGuardianStandart" },
            { GuardianSelector.Charger, "MobHoloparasiteGuardianCharger" },
            { GuardianSelector.Lighting, "MobHoloparasiteGuardianLighting" }
        };
    }
}
