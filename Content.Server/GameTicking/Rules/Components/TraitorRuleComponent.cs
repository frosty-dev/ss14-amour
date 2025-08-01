using Content.Shared._White.Mood;
using Content.Shared.Dataset;
using Content.Shared.FixedPoint;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Random;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(TraitorRuleSystem))]
public sealed partial class TraitorRuleComponent : Component
{
    public readonly List<EntityUid> TraitorMinds = new();

    [DataField]
    public ProtoId<AntagPrototype> TraitorPrototypeId = "Traitor";

    [DataField]
    public ProtoId<NpcFactionPrototype> NanoTrasenFaction = "NanoTrasen";

    [DataField]
    public ProtoId<NpcFactionPrototype> SyndicateFaction = "Syndicate";

    [DataField]
    public ProtoId<WeightedRandomPrototype> ObjectiveGroup = "TraitorObjectiveGroups";

    [DataField]
    public ProtoId<DatasetPrototype> CodewordAdjectives = "adjectives";

    [DataField]
    public ProtoId<DatasetPrototype> CodewordVerbs = "verbs";

    [DataField]
    public ProtoId<DatasetPrototype> ObjectiveIssuers = "TraitorCorporations";
    // WD EDIT START AHEAD OF WIZDEN UPSTREAM
    /// <summary>
    /// Give this traitor an Uplink on spawn.
    /// </summary>
    [DataField]
    public bool GiveUplink = true;

    /// <summary>
    /// Give this traitor the codewords.
    /// </summary>
    [DataField]
    public bool GiveCodewords = true;

    /// <summary>
    /// Give this traitor a briefing in chat.
    /// </summary>
    [DataField]
    public bool GiveBriefing = true;
    // WD EDIT END AHEAD OF WIZDEN UPSTREAM
    // WD edit start
    [DataField]
    public ProtoId<MoodEffectPrototype> MoodBuffEffect = "TraitorFocused";
    // WD edit end

    public int TotalTraitors => TraitorMinds.Count;
    public string[] Codewords = new string[3];

    public enum SelectionState
    {
        WaitingForSpawn = 0,
        ReadyToStart = 1,
        Started = 2,
    }

    /// <summary>
    /// Current state of the rule
    /// </summary>
    public SelectionState SelectionStatus = SelectionState.WaitingForSpawn;

    /// <summary>
    /// When should traitors be selected and the announcement made
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? AnnounceAt;

    /// <summary>
    ///     Path to antagonist alert sound.
    /// </summary>
    [DataField]
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/Ambience/Antag/traitor_start.ogg");

    /// <summary>
    /// The amount of codewords that are selected.
    /// </summary>
    [DataField]
    public int CodewordCount = 4;

    /// <summary>
    /// The amount of TC traitors start with.
    /// </summary>
    [DataField]
    public FixedPoint2 StartingBalance = 20; // WD EDIT AHEAD OF WIZDEN UPSTREAM

    [DataField]
    public int MaxDifficulty = 5;
}
