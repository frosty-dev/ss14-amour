using System.Diagnostics.CodeAnalysis;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Amour.Preferences.Loadouts.Effects;

/// <summary>
/// Checks for a job requirement to be met such as playtime.
/// </summary>
public sealed partial class JobRequirementLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public JobRequirement Requirement = default!;

    public override bool Validate(HumanoidCharacterProfile profile, RoleLoadout loadout, ICommonSession? session, IDependencyCollection collection, [NotNullWhen(false)] out FormattedMessage? reason)
    {
        if (session == null)
        {
            reason = FormattedMessage.Empty;
            return true;
        }

        var manager = collection.Resolve<ISharedPlaytimeManager.ISharedPlaytimeManager>();
        var playtimes = manager.GetPlayTimes(session);
        return JobRequirements.TryRequirementMet(Requirement, new Dictionary<string, TimeSpan>(playtimes), out reason,
            collection.Resolve<IEntityManager>(),
            collection.Resolve<IPrototypeManager>());
    }
}
