using System.Linq;
using Content.Shared._Amour.HumanoidAppearanceExtension;

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
        if (component.Data.Count < 2)
        {
            var erpField = component.Data.Find((x) => { return x.Name.ToLower() == "erp"; });
            var erp = new RoleplayInfo(name: "erp", roleplaySelection: erpField != null ? erpField.RoleplaySelection : RoleplaySelection.No);
            var nonconField = component.Data.Find((x) => { return x.Name.ToLower() == "noncon"; });
            var noncon = new RoleplayInfo(name: "noncon", roleplaySelection: nonconField != null ? nonconField.RoleplaySelection : RoleplaySelection.No);
            component.Data = new List<RoleplayInfo> { erp, noncon };
        }
    }
}
