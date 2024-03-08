using Content.Shared._Amour.HumanoidAppearanceExtension;
using Content.Shared.Examine;

namespace Content.Shared._Amour.RoleplayInfo;

public abstract class SharedRoleplaySystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<RoleplayInfoComponent,HumanoidAppearanceLoadingEvent>(OnHumanoidLoading);
        SubscribeLocalEvent<RoleplayInfoComponent,ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, RoleplayInfoComponent component, ExaminedEvent args)
    {
        if(component.Data.Count == 0)
            return;

        args.PushText($"\n{Loc.GetString("roleplay-info")}");

        foreach (var data in component.Data)
        {
            args.PushMarkup($"[bold]{Loc.GetString("roleplay-name-" + data.Name.ToLower())}[/bold] - {Loc.GetString("roleplay-" + data.RoleplaySelection.ToString().ToLower())}");
        }
    }

    private void OnHumanoidLoading(EntityUid uid, RoleplayInfoComponent component, HumanoidAppearanceLoadingEvent args)
    {
        Logger.Debug("LOADED SHIT! " + args.Profile.RoleplayInfoData.Count);
        component.Data = new List<RoleplayInfo>(args.Profile.RoleplayInfoData);
    }
}
