using System.Diagnostics.CodeAnalysis;
using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._White.JobRequirements;

[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class SexRequirement : JobRequirement
{
    [DataField(required: true)]
    public Sex RequiredSex;

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        if (profile is null)
            return true;

        if (!Inverted)
        {
            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-sex-whitelisted",
                ("sex", SexToString())));

            if (profile.Sex != RequiredSex)
                return false;
        }
        else
        {
            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-sex-blacklisted",
                ("sex", SexToString())));

            if (profile.Sex == RequiredSex)
                return false;
        }

        return true;
    }

    private string SexToString()
    {
        return RequiredSex switch
        {
            Sex.Male => Loc.GetString("role-timer-sex-male"),
            Sex.Female => Loc.GetString("role-timer-sex-female"),
            _ => Loc.GetString("role-timer-sex-unsexed")
        };
    }
}
