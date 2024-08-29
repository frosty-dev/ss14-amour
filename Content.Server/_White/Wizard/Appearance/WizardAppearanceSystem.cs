using Content.Server.Humanoid;
using Content.Shared._White.Wizard.Appearance;
using Content.Shared.Dataset;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._White.Wizard.Appearance;

public sealed class WizardAppearanceSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WizardAppearanceComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid wizard, WizardAppearanceComponent appearance, ComponentInit _)
    {
        Wizardify(wizard, appearance);
    }

    public void Wizardify(EntityUid wizard, WizardAppearanceComponent appearance)
    {
        var profile = GetWizardProfile(appearance);

        _humanoid.LoadProfile(wizard, profile);

        _metaData.SetEntityName(wizard, GetRandom(appearance.Name, string.Empty));
    }

    public string GetWizardName(WizardAppearanceComponent appearance)
    {
        return GetRandom(appearance.Name, string.Empty);
    }

    public EntityUid GetWizardEntity(WizardAppearanceComponent appearance)
    {
        var profile = GetWizardProfile(appearance);

        if (!_prototypeManager.TryIndex(profile.Species, out SpeciesPrototype? species))
            return EntityUid.Invalid;

        var entity = Spawn(species.Prototype);

        _humanoid.LoadProfile(entity, profile);
        _metaData.SetEntityName(entity, GetWizardName(appearance));

        return entity;
    }

    public HumanoidCharacterProfile GetWizardProfile(WizardAppearanceComponent appearance)
    {
        var profile = HumanoidCharacterProfile.RandomWithSpecies().WithAge(_random.Next(appearance.MinAge, appearance.MaxAge));

        var color = Color.FromHex(GetRandom(appearance.Color, "#B5B8B1"));
        var hair = GetRandom(appearance.Hair, "HumanHairAfricanPigtails");
        var facialHair = GetRandom(appearance.FacialHair, "HumanFacialHairAbe");
        profile = profile.WithCharacterAppearance( // holy shit
            profile.WithCharacterAppearance(
                    profile.WithCharacterAppearance(
                            profile.WithCharacterAppearance(
                                    profile.Appearance.WithHairStyleName(hair))
                                .Appearance.WithFacialHairStyleName(facialHair))
                        .Appearance.WithHairColor(color))
                .Appearance.WithFacialHairColor(color));

        return profile;
    }

    private string GetRandom(string list, string ifNull)
    {
        return _prototypeManager.TryIndex<DatasetPrototype>(list, out var prototype)
            ? _random.Pick(prototype.Values)
            : ifNull;
    }
}
