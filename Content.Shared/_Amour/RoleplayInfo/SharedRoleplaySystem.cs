using System.Linq;
using Content.Shared._Amour.HumanoidAppearanceExtension;
using Content.Shared.Examine;
using Content.Shared.Humanoid;

namespace Content.Shared._Amour.RoleplayInfo;

public abstract class SharedRoleplaySystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<RoleplayInfoComponent, HumanoidAppearanceLoadingEvent>(OnHumanoidLoading);
    }

    private void OnHumanoidLoading(EntityUid uid, RoleplayInfoComponent component, HumanoidAppearanceLoadingEvent args)
    {
        component.Data = new List<RoleplayInfo>(args.Profile.RoleplayInfoData.Select(p => p.Value));
    }
}
