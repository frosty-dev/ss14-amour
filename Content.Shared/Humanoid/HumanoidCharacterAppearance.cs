using System.Linq;
using Content.Shared._Amour.Hole;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.Humanoid;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class HumanoidCharacterAppearance(
    string hairStyleId,
    Color hairColor,
    string facialHairStyleId,
    Color facialHairColor,
    Color eyeColor,
    Color skinColor,
    List<Marking> markings,
    byte height,
    List<Genital> gens)
    : ICharacterAppearance
{
    [DataField("hair")]
    public string HairStyleId { get; private set; } = hairStyleId;

    [DataField("hairColor")]
    public Color HairColor { get; private set; } = ClampColor(hairColor);

    [DataField("facialHair")]
    public string FacialHairStyleId { get; private set; } = facialHairStyleId;

    [DataField("facialHairColor")]
    public Color FacialHairColor { get; private set; } = ClampColor(facialHairColor);

    [DataField("eyeColor")]
    public Color EyeColor { get; private set; } = ClampColor(eyeColor);

    [DataField("skinColor")]
    public Color SkinColor { get; private set; } = ClampColor(skinColor);

    [DataField("markings")]
    public List<Marking> Markings { get; private set; } = markings;

    [DataField("genitals")]
    public List<Genital> Genitals { get; private set; } = gens; //AMOUR

    [DataField]
    public byte Height { get; private set; } = height; //AMOUR

    public HumanoidCharacterAppearance WithHairStyleName(string newName)
    {
        return new HumanoidCharacterAppearance(newName, HairColor, FacialHairStyleId, FacialHairColor, EyeColor,
            SkinColor, Markings, Height, Genitals);
    }

    public HumanoidCharacterAppearance WithHairColor(Color newColor)
    {
        return new HumanoidCharacterAppearance(HairStyleId, newColor, FacialHairStyleId, FacialHairColor, EyeColor,
            SkinColor, Markings, Height, Genitals);
    }

    public HumanoidCharacterAppearance WithFacialHairStyleName(string newName)
    {
        return new HumanoidCharacterAppearance(HairStyleId, HairColor, newName, FacialHairColor, EyeColor,
            SkinColor, Markings, Height, Genitals);
    }

    public HumanoidCharacterAppearance WithFacialHairColor(Color newColor)
    {
        return new HumanoidCharacterAppearance(HairStyleId, HairColor, FacialHairStyleId, newColor, EyeColor,
            SkinColor, Markings, Height, Genitals);
    }

    public HumanoidCharacterAppearance WithEyeColor(Color newColor)
    {
        return new HumanoidCharacterAppearance(HairStyleId, HairColor, FacialHairStyleId, FacialHairColor, newColor,
            SkinColor, Markings, Height, Genitals);
    }

    public HumanoidCharacterAppearance WithSkinColor(Color newColor)
    {
        return new HumanoidCharacterAppearance(HairStyleId, HairColor, FacialHairStyleId, FacialHairColor, EyeColor,
            newColor, Markings, Height, Genitals);
    }

    public HumanoidCharacterAppearance WithMarkings(List<Marking> newMarkings)
    {
        return new HumanoidCharacterAppearance(HairStyleId, HairColor, FacialHairStyleId, FacialHairColor, EyeColor,
            SkinColor, newMarkings, Height, Genitals);
    }

    public HumanoidCharacterAppearance WithGenitals(List<Genital> genitals)
    {
        return new(HairStyleId, HairColor, FacialHairStyleId, FacialHairColor, EyeColor, SkinColor, Markings, Height,
            genitals);
    }

    public HumanoidCharacterAppearance WithHeight(byte height)
    {
        return new(HairStyleId, HairColor, FacialHairStyleId, FacialHairColor, EyeColor, SkinColor, Markings, height,
            Genitals);
    }

    public HumanoidCharacterAppearance() : this(
        HairStyles.DefaultHairStyle,
        Color.Black,
        HairStyles.DefaultFacialHairStyle,
        Color.Black,
        Color.Black,
        Humanoid.SkinColor.ValidHumanSkinTone,
        [],
        128,
        []
    )
    {
    }

    public static string DefaultWithBodyType(string species)
    {
        var speciesPrototype = IoCManager.Resolve<IPrototypeManager>().Index<SpeciesPrototype>(species);

        return speciesPrototype.BodyTypes.First();
    }

    public static HumanoidCharacterAppearance DefaultWithSpecies(string species)
    {
        var speciesPrototype = IoCManager.Resolve<IPrototypeManager>().Index<SpeciesPrototype>(species);
        var skinColor = speciesPrototype.SkinColoration switch
        {
            HumanoidSkinColor.HumanToned => Humanoid.SkinColor.HumanSkinTone(speciesPrototype.DefaultHumanSkinTone),
            HumanoidSkinColor.Hues       => speciesPrototype.DefaultSkinTone,
            HumanoidSkinColor.TintedHues => Humanoid.SkinColor.TintedHues(speciesPrototype.DefaultSkinTone),
            _                            => Humanoid.SkinColor.ValidHumanSkinTone
        };

        return new HumanoidCharacterAppearance(
            HairStyles.DefaultHairStyle,
            Color.Black,
            HairStyles.DefaultFacialHairStyle,
            Color.Black,
            Color.Black,
            skinColor,
            [],
            128,
            []
        );
    }

    private static IReadOnlyList<Color> _realisticEyeColors = new List<Color>
    {
        Color.Brown,
        Color.Gray,
        Color.Azure,
        Color.SteelBlue,
        Color.Black
    };

    public static HumanoidCharacterAppearance Random(string species, Sex sex)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var markingManager = IoCManager.Resolve<MarkingManager>();
        var hairStyles = markingManager.MarkingsByCategoryAndSpecies(MarkingCategories.Hair, species).Keys.ToList();
        var facialHairStyles = markingManager.MarkingsByCategoryAndSpecies(MarkingCategories.FacialHair, species)
            .Keys.ToList();

        var newHairStyle = hairStyles.Count > 0
            ? random.Pick(hairStyles)
            : HairStyles.DefaultHairStyle;

        var newFacialHairStyle = facialHairStyles.Count == 0 || sex == Sex.Female
            ? HairStyles.DefaultFacialHairStyle
            : random.Pick(facialHairStyles);

        var newHairColor = random.Pick(HairStyles.RealisticHairColors);
        newHairColor = newHairColor
            .WithRed(RandomizeColor(newHairColor.R))
            .WithGreen(RandomizeColor(newHairColor.G))
            .WithBlue(RandomizeColor(newHairColor.B));

        // TODO: Add random markings

        var newEyeColor = random.Pick(_realisticEyeColors);

        var skinType = IoCManager.Resolve<IPrototypeManager>().Index<SpeciesPrototype>(species).SkinColoration;

        var newSkinColor = Humanoid.SkinColor.ValidHumanSkinTone;
        switch (skinType)
        {
            case HumanoidSkinColor.HumanToned:
                var tone = random.Next(0, 100);
                newSkinColor = Humanoid.SkinColor.HumanSkinTone(tone);
                break;
            case HumanoidSkinColor.Hues:
            case HumanoidSkinColor.TintedHues:
                var rbyte = random.NextByte();
                var gbyte = random.NextByte();
                var bbyte = random.NextByte();
                newSkinColor = new Color(rbyte, gbyte, bbyte);
                break;
        }

        if (skinType == HumanoidSkinColor.TintedHues)
        {
            newSkinColor = Humanoid.SkinColor.ValidTintedHuesSkinTone(newSkinColor);
        }

        return new HumanoidCharacterAppearance(newHairStyle, newHairColor, newFacialHairStyle, newHairColor,
            newEyeColor, newSkinColor, [], 128, []);

        float RandomizeColor(float channel)
        {
            return MathHelper.Clamp01(channel + random.Next(-25, 25) / 100f);
        }
    }

    public static Color ClampColor(Color color)
    {
        return new Color(color.RByte, color.GByte, color.BByte);
    }

    public static HumanoidCharacterAppearance EnsureValid(
        HumanoidCharacterAppearance appearance,
        string species,
        string bodyType,
        string[] sponsorMarkings) //WD-EDIT
    {
        var hairStyleId = appearance.HairStyleId;
        var facialHairStyleId = appearance.FacialHairStyleId;

        var hairColor = ClampColor(appearance.HairColor);
        var facialHairColor = ClampColor(appearance.FacialHairColor);
        var eyeColor = ClampColor(appearance.EyeColor);

        var proto = IoCManager.Resolve<IPrototypeManager>();
        var markingManager = IoCManager.Resolve<MarkingManager>();

        if (!markingManager.MarkingsByCategory(MarkingCategories.Hair).ContainsKey(hairStyleId))
        {
            hairStyleId = HairStyles.DefaultHairStyle;
        }

        // WD-EDIT
        if (proto.TryIndex(hairStyleId, out MarkingPrototype? hairProto) &&
            hairProto.SponsorOnly &&
            !sponsorMarkings.Contains(hairStyleId))
        {
            hairStyleId = HairStyles.DefaultHairStyle;
        }
        // WD-EDIT

        if (!markingManager.MarkingsByCategory(MarkingCategories.FacialHair).ContainsKey(facialHairStyleId))
        {
            facialHairStyleId = HairStyles.DefaultFacialHairStyle;
        }

        // WD-EDIT
        if (proto.TryIndex(facialHairStyleId, out MarkingPrototype? facialHairProto) &&
            facialHairProto.SponsorOnly &&
            !sponsorMarkings.Contains(facialHairStyleId))
        {
            facialHairStyleId = HairStyles.DefaultFacialHairStyle;
        }
        // WD-EDIT

        // AMOUR EDIT
        var genitals = new List<Genital>();
        foreach (var genital in appearance.Genitals)
        {
            if (proto.TryIndex(genital.GenitalId, out _))
                genitals.Add(genital);
        }
        // END AMOUR EDIT

        var markingSet = new MarkingSet();
        var skinColor = appearance.SkinColor;
        if (!proto.TryIndex(species, out SpeciesPrototype? speciesProto))
        {
            return new HumanoidCharacterAppearance(
                hairStyleId,
                hairColor,
                facialHairStyleId,
                facialHairColor,
                eyeColor,
                skinColor,
                markingSet.GetForwardEnumerator().ToList(), appearance.Height, genitals);
        }

        markingSet = new MarkingSet(appearance.Markings, speciesProto.MarkingPoints, markingManager, proto);
        markingSet.EnsureValid(markingManager);

        if (!Humanoid.SkinColor.VerifySkinColor(speciesProto.SkinColoration, skinColor))
        {
            skinColor = Humanoid.SkinColor.ValidSkinTone(speciesProto.SkinColoration, skinColor);
        }

        markingSet.EnsureSpecies(species, bodyType, skinColor, markingManager);

        // WD-EDIT
        markingSet.FilterSponsor(sponsorMarkings, markingManager);
        // WD-EDIT

        return new HumanoidCharacterAppearance(
            hairStyleId,
            hairColor,
            facialHairStyleId,
            facialHairColor,
            eyeColor,
            skinColor,
            markingSet.GetForwardEnumerator().ToList(),
            appearance.Height, genitals);
    }

    public bool MemberwiseEquals(ICharacterAppearance maybeOther)
    {
        if (maybeOther is not HumanoidCharacterAppearance other)
            return false;

        if (HairStyleId != other.HairStyleId)
            return false;

        if (!HairColor.Equals(other.HairColor))
            return false;

        if (FacialHairStyleId != other.FacialHairStyleId)
            return false;

        if (!FacialHairColor.Equals(other.FacialHairColor))
            return false;

        if (!EyeColor.Equals(other.EyeColor))
            return false;

        return SkinColor.Equals(other.SkinColor) && Markings.SequenceEqual(other.Markings);
    }
}
