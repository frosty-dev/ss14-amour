using System.Linq;
using Content.Shared.Humanoid;
using Content.Shared.White.RolePlayThink;
using Content.Shared.White.ShittyInteraction;

namespace Content.Server.Humanoid;

public sealed partial class HumanoidAppearanceSystem
{
    private void SetRoleplayThink(EntityUid uid, IReadOnlyDictionary<string, RoleplaySelection> profileRoleplaySelections,
        HumanoidAppearanceComponent humanoid)
    {
        if(!TryComp<InteractibleComponent>(uid, out var interactibleComponent))
            return;

        interactibleComponent.Preferences = profileRoleplaySelections.ToDictionary(k => k.Key,k=> k.Value);
        Dirty(interactibleComponent);
    }
}
