using Content.Server.Humanoid;
using Content.Shared._White.Wizard.Appearance;
using Content.Shared.Dataset;
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
        var random = IoCManager.Resolve<IRobustRandom>();
        var profile = HumanoidCharacterProfile.RandomWithSpecies().WithAge(random.Next(appearance.MinAge, appearance.MaxAge));

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

        _humanoid.LoadProfile(wizard, profile);

        _metaData.SetEntityName(wizard, GetRandom(appearance.Name, string.Empty));
    }

    private string GetRandom(string list, string ifNull)
    {
        return _prototypeManager.TryIndex<DatasetPrototype>(list, out var prototype)
            ? _random.Pick(prototype.Values)
            : ifNull;
    }
}
