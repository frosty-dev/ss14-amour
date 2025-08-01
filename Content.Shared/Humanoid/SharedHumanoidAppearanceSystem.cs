using System.Linq;
using Content.Shared._Amour.Hole;
using Content.Shared._Amour.HumanoidAppearanceExtension;
using Content.Shared._White.TTS;
using Content.Shared.Examine;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid;

/// <summary>
///     HumanoidSystem. Primarily deals with the appearance and visual data
///     of a humanoid entity. HumanoidVisualizer is what deals with actually
///     organizing the sprites and setting up the sprite component's layers.
///
///     This is a shared system, because while it is server authoritative,
///     you still need a local copy so that players can set up their
///     characters.
/// </summary>
public abstract class SharedHumanoidAppearanceSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly SharedHoleSystem _holeSystem = default!;

    [ValidatePrototypeId<SpeciesPrototype>]
    public const string DefaultSpecies = "Human";

    [ValidatePrototypeId<BodyTypePrototype>]
    public const string DefaultBodyType = "HumanNormal";

    [ValidatePrototypeId<TTSVoicePrototype>]
    public const string DefaultVoice = "Nord";

    public static readonly Dictionary<Sex, ProtoId<TTSVoicePrototype>> DefaultSexVoice = new()
    {
        { Sex.Male, "Nord" },
        { Sex.Female, "Amina" },
        { Sex.Unsexed, "Alyx" },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HumanoidAppearanceComponent, ExaminedEvent>(OnExamined);
    }

    private void OnInit(EntityUid uid, HumanoidAppearanceComponent humanoid, ComponentInit args)
    {
        if (string.IsNullOrEmpty(humanoid.Species) || _netManager.IsClient && !IsClientSide(uid))
        {
            return;
        }

        if (string.IsNullOrEmpty(humanoid.Initial)
            || !_proto.TryIndex(humanoid.Initial, out HumanoidProfilePrototype? startingSet))
        {
            LoadProfile(uid, HumanoidCharacterProfile.DefaultWithSpecies(humanoid.Species), humanoid);
            return;
        }

        // Do this first, because profiles currently do not support custom base layers
        foreach (var (layer, info) in startingSet.CustomBaseLayers)
        {
            humanoid.CustomBaseLayers.Add(layer, info);
        }

        LoadProfile(uid, startingSet.Profile, humanoid);
    }

    private void OnExamined(EntityUid uid, HumanoidAppearanceComponent component, ExaminedEvent args)
    {
        var identity = Identity.Entity(uid, EntityManager);
        var species = GetSpeciesRepresentation(component.Species).ToLower();
        var age = GetAgeRepresentation(component.Species, component.Age);

        args.PushText(Loc.GetString("humanoid-appearance-component-examine", ("user", identity), ("age", age), ("species", species)));
    }

    /// <summary>
    ///     Toggles a humanoid's sprite layer visibility.
    /// </summary>
    /// <param name="uid">Humanoid mob's UID</param>
    /// <param name="layer">Layer to toggle visibility for</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetLayerVisibility(
        EntityUid uid,
        HumanoidVisualLayers layer,
        bool visible,
        bool permanent = false,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid, false))
            return;

        var dirty = false;
        SetLayerVisibility(uid, humanoid, layer, visible, permanent, ref dirty);
        if (dirty)
            Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Sets the visibility for multiple layers at once on a humanoid's sprite.
    /// </summary>
    /// <param name="uid">Humanoid mob's UID</param>
    /// <param name="layers">An enumerable of all sprite layers that are going to have their visibility set</param>
    /// <param name="visible">The visibility state of the layers given</param>
    /// <param name="permanent">If this is a permanent change, or temporary. Permanent layers are stored in their own hash set.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetLayersVisibility(
        EntityUid uid,
        IEnumerable<HumanoidVisualLayers> layers,
        bool visible,
        bool permanent = false,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid))
            return;

        var dirty = false;

        foreach (var layer in layers)
        {
            SetLayerVisibility(uid, humanoid, layer, visible, permanent, ref dirty);
        }

        if (dirty)
            Dirty(uid, humanoid);
    }

    protected virtual void SetLayerVisibility(
        EntityUid uid,
        HumanoidAppearanceComponent humanoid,
        HumanoidVisualLayers layer,
        bool visible,
        bool permanent,
        ref bool dirty)
    {
        if (visible)
        {
            if (permanent)
                dirty |= humanoid.PermanentlyHidden.Remove(layer);

            dirty |= humanoid.HiddenLayers.Remove(layer);
        }
        else
        {
            if (permanent)
                dirty |= humanoid.PermanentlyHidden.Add(layer);

            dirty |= humanoid.HiddenLayers.Add(layer);
        }
    }

    /// <summary>
    ///     Set a humanoid mob's species. This will change their base sprites, as well as their current
    ///     set of markings to fit against the mob's new species.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="species">The species to set the mob to. Will return if the species prototype was invalid.</param>
    /// <param name="sync">Whether to immediately synchronize this to the humanoid mob, or not.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetSpecies(
        EntityUid uid,
        string species,
        bool sync = true,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) || !_proto.TryIndex<SpeciesPrototype>(species, out var prototype))
        {
            return;
        }

        humanoid.Species = species;
        humanoid.MarkingSet.EnsureSpecies(species, humanoid.BodyType, humanoid.SkinColor, _markingManager);
        var oldMarkings = humanoid.MarkingSet.GetForwardEnumerator().ToList();
        humanoid.MarkingSet = new MarkingSet(oldMarkings, prototype.MarkingPoints, _markingManager, _proto);

        if (sync)
            Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Sets the skin color of this humanoid mob. Will only affect base layers that are not custom,
    ///     custom base layers should use <see cref="SetBaseLayerColor"/> instead.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="skinColor">Skin color to set on the humanoid mob.</param>
    /// <param name="sync">Whether to synchronize this to the humanoid mob, or not.</param>
    /// <param name="verify">Whether to verify the skin color can be set on this humanoid or not</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public virtual void SetSkinColor(
        EntityUid uid,
        Color skinColor,
        bool sync = true,
        bool verify = true,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid))
            return;

        if (!_proto.TryIndex(humanoid.Species, out var species))
        {
            return;
        }

        if (verify && !SkinColor.VerifySkinColor(species.SkinColoration, skinColor))
        {
            skinColor = SkinColor.ValidSkinTone(species.SkinColoration, skinColor);
        }

        humanoid.SkinColor = skinColor;

        if (sync)
            Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Set a humanoid mob's body yupe. This will change their base sprites.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="bodyType">The body type to set the mob to. Will return if the body type prototype was invalid.</param>
    /// <param name="sync">Whether to immediately synchronize this to the humanoid mob, or not.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetBodyType(
        EntityUid uid,
        ProtoId<BodyTypePrototype> bodyType,
        bool sync = true,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) || !_proto.TryIndex(bodyType, out _))
        {
            return;
        }

        humanoid.BodyType = bodyType;

        if (sync)
        {
            Dirty(uid, humanoid);
        }
    }

    /// <summary>
    ///     Set a humanoid mob's body yupe. This will change their base sprites.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="voiceId">The tts voice to set the mob to.</param>
    /// <param name="sync">Whether to immediately synchronize this to the humanoid mob, or not.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    // ReSharper disable once InconsistentNaming
    public void SetTTSVoice(
        EntityUid uid,
        ProtoId<TTSVoicePrototype> voiceId,
        bool sync = true,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!TryComp<SharedTTSComponent>(uid, out var comp))
            return;

        if (!Resolve(uid, ref humanoid))
        {
            return;
        }

        humanoid.Voice = voiceId;
        comp.VoicePrototypeId = voiceId;
        if (sync)
        {
            Dirty(uid, humanoid);
        }
    }

    /// <summary>
    ///     Sets the base layer ID of this humanoid mob. A humanoid mob's 'base layer' is
    ///     the skin sprite that is applied to the mob's sprite upon appearance refresh.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="layer">The layer to target on this humanoid mob.</param>
    /// <param name="id">The ID of the sprite to use. See <see cref="HumanoidSpeciesSpriteLayer"/>.</param>
    /// <param name="sync">Whether to synchronize this to the humanoid mob, or not.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetBaseLayerId(
        EntityUid uid,
        HumanoidVisualLayers layer,
        string? id,
        bool sync = true,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid))
            return;

        if (humanoid.CustomBaseLayers.TryGetValue(layer, out var info))
            humanoid.CustomBaseLayers[layer] = info with { Id = id };
        else
            humanoid.CustomBaseLayers[layer] = new(id);

        if (sync)
            Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Sets the color of this humanoid mob's base layer. See <see cref="SetBaseLayerId"/> for a
    ///     description of how base layers work.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="layer">The layer to target on this humanoid mob.</param>
    /// <param name="color">The color to set this base layer to.</param>
    public void SetBaseLayerColor(
        EntityUid uid,
        HumanoidVisualLayers layer,
        Color? color,
        bool sync = true,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid))
            return;

        if (humanoid.CustomBaseLayers.TryGetValue(layer, out var info))
            humanoid.CustomBaseLayers[layer] = info with { Color = color };
        else
            humanoid.CustomBaseLayers[layer] = new(null, color);

        if (sync)
            Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Set a humanoid mob's sex. This will not change their gender.
    /// </summary>
    /// <param name="uid">The humanoid mob's UID.</param>
    /// <param name="sex">The sex to set the mob to.</param>
    /// <param name="sync">Whether to immediately synchronize this to the humanoid mob, or not.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void SetSex(EntityUid uid, Sex sex, bool sync = true, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) || humanoid.Sex == sex)
            return;

        var oldSex = humanoid.Sex;
        humanoid.Sex = sex;
        humanoid.MarkingSet.EnsureSexes(sex, _markingManager);
        RaiseLocalEvent(uid, new SexChangedEvent(oldSex, sex));

        if (sync)
        {
            Dirty(uid, humanoid);
        }
    }

    public void SwapSex(EntityUid uid, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) || humanoid.Sex == Sex.Unsexed)
            return;

        // Not set up for future possible alien sexes
        if (humanoid.Sex == Sex.Male)
        {
            SetSex(uid,Sex.Female);
            return;
        }

        SetSex(uid,Sex.Male);
    }

    public List<BodyTypePrototype> GetValidBodyTypes(SpeciesPrototype species, Sex sex)
    {
        return species.BodyTypes.Select(protoId => _proto.Index<BodyTypePrototype>(protoId))
            .Where(proto => !proto.SexRestrictions.Contains(sex.ToString())).ToList();
    }

    public static bool IsBodyTypeValid(BodyTypePrototype bodyType, SpeciesPrototype species, Sex sex)
    {
        return species.BodyTypes.Contains(bodyType.ID);
    }

    /// <summary>
    ///     Loads a humanoid character profile directly onto this humanoid mob.
    /// </summary>
    /// <param name="uid">The mob's entity UID.</param>
    /// <param name="profile">The character profile to load.</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public virtual void LoadProfile(EntityUid uid, HumanoidCharacterProfile? profile, HumanoidAppearanceComponent? humanoid = null)
    {
        if (profile == null)
            return;

        if (!Resolve(uid, ref humanoid))
        {
            return;
        }

        // AMOUR START
        var ev = new HumanoidAppearanceLoadingEvent(new Entity<HumanoidAppearanceComponent>(uid, humanoid), profile);
        RaiseLocalEvent(uid, ev);
        // AMOUR END

        SetSpecies(uid, profile.Species, false, humanoid);
        SetSex(uid, profile.Sex, false, humanoid);
        humanoid.EyeColor = profile.Appearance.EyeColor;

        SetSkinColor(uid, profile.Appearance.SkinColor, false);
        SetBodyType(uid, profile.BodyType, false, humanoid);
        SetTTSVoice(uid, profile.Voice, false, humanoid);

        //AMOUR
        foreach (var genitals in profile.Appearance.Genitals)
        {
            _holeSystem.AddHole(uid,genitals.GenitalId,genitals.Color);
        }

        humanoid.MarkingSet.Clear();

        // Add markings that doesn't need coloring. We store them until we add all other markings that doesn't need it.
        var markingFColored = new Dictionary<Marking, MarkingPrototype>();
        foreach (var marking in profile.Appearance.Markings)
        {
            if (_markingManager.TryGetMarking(marking, out var prototype))
            {
                if (!prototype.ForcedColoring)
                {
                    AddMarking(uid, marking.MarkingId, marking.MarkingColors, false);
                }
                else
                {
                    markingFColored.Add(marking, prototype);
                }
            }
        }

        // Hair/facial hair - this may eventually be deprecated.
        // We need to ensure hair before applying it or coloring can try depend on markings that can be invalid
        var hairColor = _markingManager.MustMatchSkin(profile.Species, HumanoidVisualLayers.Hair, out var hairAlpha,
            _proto)
            ? profile.Appearance.SkinColor.WithAlpha(hairAlpha)
            : profile.Appearance.HairColor;

        var facialHairColor = _markingManager.MustMatchSkin(profile.Species, HumanoidVisualLayers.FacialHair,
            out var facialHairAlpha, _proto)
            ? profile.Appearance.SkinColor.WithAlpha(facialHairAlpha)
            : profile.Appearance.FacialHairColor;

        if (_markingManager.Markings.TryGetValue(profile.Appearance.HairStyleId, out var hairPrototype) &&
            _markingManager.CanBeApplied(profile.Species, profile.Sex, hairPrototype, _proto))
        {
            AddMarking(uid, profile.Appearance.HairStyleId, hairColor, false);
        }

        if (_markingManager.Markings.TryGetValue(profile.Appearance.FacialHairStyleId, out var facialHairPrototype) &&
            _markingManager.CanBeApplied(profile.Species, profile.Sex, facialHairPrototype, _proto))
        {
            AddMarking(uid, profile.Appearance.FacialHairStyleId, facialHairColor, false);
        }

        humanoid.MarkingSet.EnsureSpecies(profile.Species, profile.BodyType, profile.Appearance.SkinColor,
            _markingManager,
            _proto);

        // Finally adding marking with forced colors
        foreach (var (marking, prototype) in markingFColored)
        {
            var markingColors = MarkingColoring.GetMarkingLayerColors(
                prototype,
                profile.Appearance.SkinColor,
                profile.Appearance.EyeColor,
                humanoid.MarkingSet
            );

            AddMarking(uid, marking.MarkingId, markingColors, false);
        }

        EnsureDefaultMarkings(uid, humanoid);

        humanoid.Gender = profile.Gender;
        if (TryComp<GrammarComponent>(uid, out var grammar))
        {
            grammar.Gender = profile.Gender;
        }

        humanoid.Age = profile.Age;

        // AMOUR START
        var ev2 = new HumanoidAppearanceLoadedEvent(new Entity<HumanoidAppearanceComponent>(uid, humanoid), profile);
        RaiseLocalEvent(uid, ev2);
        // AMOUR END

        Dirty(uid, humanoid);
    }

    /// <summary>
    ///     Adds a marking to this humanoid.
    /// </summary>
    /// <param name="uid">Humanoid mob's UID</param>
    /// <param name="marking">Marking ID to use</param>
    /// <param name="color">Color to apply to all marking layers of this marking</param>
    /// <param name="sync">Whether to immediately sync this marking or not</param>
    /// <param name="forced">If this marking was forced (ignores marking points)</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void AddMarking(
        EntityUid uid,
        string marking,
        Color? color = null,
        bool sync = true,
        bool forced = false,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid)
            || !_markingManager.Markings.TryGetValue(marking, out var prototype))
        {
            return;
        }

        var markingObject = prototype.AsMarking();
        markingObject.Forced = forced;
        if (color != null)
        {
            for (var i = 0; i < prototype.Sprites.Count; i++)
            {
                markingObject.SetColor(i, color.Value);
            }
        }

        humanoid.MarkingSet.AddBack(prototype.MarkingCategory, markingObject);

        if (sync)
            Dirty(uid, humanoid);
    }

    private void EnsureDefaultMarkings(EntityUid uid, HumanoidAppearanceComponent? humanoid)
    {
        if (!Resolve(uid, ref humanoid))
        {
            return;
        }

        humanoid.MarkingSet.EnsureDefault(humanoid.SkinColor, humanoid.EyeColor, _markingManager);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="uid">Humanoid mob's UID</param>
    /// <param name="marking">Marking ID to use</param>
    /// <param name="colors">Colors to apply against this marking's set of sprites.</param>
    /// <param name="sync">Whether to immediately sync this marking or not</param>
    /// <param name="forced">If this marking was forced (ignores marking points)</param>
    /// <param name="humanoid">Humanoid component of the entity</param>
    public void AddMarking(
        EntityUid uid,
        string marking,
        IReadOnlyList<Color> colors,
        bool sync = true,
        bool forced = false,
        HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid)
            || !_markingManager.Markings.TryGetValue(marking, out var prototype))
        {
            return;
        }

        var markingObject = new Marking(marking, colors)
        {
            Forced = forced
        };

        humanoid.MarkingSet.AddBack(prototype.MarkingCategory, markingObject);

        if (sync)
            Dirty(uid, humanoid);
    }

    /// <summary>
    /// Takes ID of the species prototype, returns UI-friendly name of the species.
    /// </summary>
    public string GetSpeciesRepresentation(string speciesId)
    {
        if (_proto.TryIndex<SpeciesPrototype>(speciesId, out var species))
        {
            return Loc.GetString(species.Name);
        }

        Log.Error("Tried to get representation of unknown species: {speciesId}");
        return Loc.GetString("humanoid-appearance-component-unknown-species");
    }

    public string GetAgeRepresentation(string species, int age)
    {
        if (!_proto.TryIndex<SpeciesPrototype>(species, out var speciesPrototype))
        {
            Log.Error("Tried to get age representation of species that couldn't be indexed: " + species);
            return Loc.GetString("identity-age-young");
        }

        if (age < speciesPrototype.YoungAge)
        {
            return Loc.GetString("identity-age-young");
        }

        if (age < speciesPrototype.OldAge)
        {
            return Loc.GetString("identity-age-middle-aged");
        }

        return Loc.GetString("identity-age-old");
    }
}
