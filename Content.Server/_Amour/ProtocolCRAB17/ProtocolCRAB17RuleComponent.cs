using Robust.Shared.Audio;

namespace Content.Server._Amour.ProtocolCRAB17;

[RegisterComponent]
public sealed partial class ProtocolCRAB17RuleComponent : Component
{
    /// <summary>
    /// Minimal time between events.
    /// </summary>
    public static TimeSpan TimeBetweenEvents = TimeSpan.FromMinutes(20);

    public static SoundSpecifier Announcement = new SoundPathSpecifier("/Audio/_Amour/ProtocolCRAB17/Bogdanoff_is_calling.ogg");

    [ViewVariables]
    public EntityUid? TargetStation;
    [ViewVariables]
    public HashSet<int> Callers = new();
}
