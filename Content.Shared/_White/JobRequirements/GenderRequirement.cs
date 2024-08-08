using System.Diagnostics.CodeAnalysis;
using System.Text;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._White.JobRequirements;

[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class GenderRequirement : JobRequirement
{
    [DataField(required: true)]
    public HashSet<Gender> RequiredGenders;

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        if (profile is null)
            return true;

        var sb = new StringBuilder();
        sb.Append("[color=yellow]");
        foreach (var g in RequiredGenders)
        {
            sb.Append(GenderToString(g) + " ");
        }

        sb.Append("[/color]");

        if (!Inverted)
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-gender-whitelisted")}\n{sb}");

            if (!RequiredGenders.Contains(profile.Gender))
                return false;
        }
        else
        {
            reason = FormattedMessage.FromMarkupPermissive($"{Loc.GetString("role-timer-gender-blacklisted")}\n{sb}");

            if (RequiredGenders.Contains(profile.Gender))
                return false;
        }

        return true;
    }

    private string GenderToString(Gender gender)
    {
        return gender switch
        {
            Gender.Male => Loc.GetString("humanoid-profile-editor-pronouns-male-text"),
            Gender.Female => Loc.GetString("humanoid-profile-editor-pronouns-female-text"),
            Gender.Epicene => Loc.GetString("humanoid-profile-editor-pronouns-epicene-text"),
            _ => Loc.GetString("humanoid-profile-editor-pronouns-neuter-text")
        };
    }
}
