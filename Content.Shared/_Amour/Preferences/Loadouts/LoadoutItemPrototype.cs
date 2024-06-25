using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared._Amour.Preferences.Loadouts;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype()]
public sealed class LoadoutItemPrototype : IPrototype, IInheritingPrototype
{

    [IdDataField]
    public string ID { get; } = default!;


    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<LoadoutItemPrototype>))]
    public string[]? Parents { get; }


    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }

    // WD-Sponsors-Start
    [DataField("sponsorOnly")]
    public bool SponsorOnly = false;

    [DataField("whitelistJobs", customTypeSerializer: typeof(PrototypeIdListSerializer<JobPrototype>))]
    public List<string>? WhitelistJobs { get; }

    [DataField("blacklistJobs", customTypeSerializer: typeof(PrototypeIdListSerializer<JobPrototype>))]
    public List<string>? BlacklistJobs { get; }

    [DataField("speciesRestriction")]
    public List<string>? SpeciesRestrictions { get; }

    [DataField("entity", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string EntityId { get; } = default!;
    // WD-Sponsors-End
}
