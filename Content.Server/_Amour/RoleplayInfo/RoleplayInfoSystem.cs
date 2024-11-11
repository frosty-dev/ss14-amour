using Content.Server.Humanoid;
using Content.Shared._Amour.RoleplayInfo;
using Content.Shared.Examine;

namespace Content.Server._Amour.RoleplayInfo;

public sealed class RoleplayInfoSystem : SharedRoleplaySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoleplayInfoComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, RoleplayInfoComponent component, ExaminedEvent args)
    {
        if (component.Data.Count == 0)
            return;

        args.PushText($"\n{Loc.GetString("roleplay-info")}", -1);

        foreach (var data in component.Data)
        {
            args.PushMarkup($"[bold]{Loc.GetString("roleplay-name-" + data.Name.ToLower())}[/bold] - {Loc.GetString("roleplay-" + data.RoleplaySelection.ToString().ToLower())}", -1);
        }
    }
}
