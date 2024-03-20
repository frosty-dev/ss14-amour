using Content.Server.Administration;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared._Amour.Hole;
using Content.Shared.Administration;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Server._Amour.Hole;

public sealed partial class HoleSystem : SharedHoleSystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeInventory();

        SubscribeLocalEvent<HoleComponent,ComponentGetState>(OnGetState);
    }

    private void OnGetState(EntityUid uid, HoleComponent component,ref ComponentGetState args)
    {
        args.State = new HoleComponentState(component.Parent, component.IsExcited);
    }

    public override void Update(float frameTime)
    {
        UpdateSolution(frameTime);
    }
}

[ToolshedCommand, AdminCommand(AdminFlags.Fun)]
internal sealed class AddHoleCommand : ToolshedCommand
{
    [CommandImplementation]
    public void AddHole(
        [CommandInvocationContext] IInvocationContext ctx,
        [PipedArgument] EntityUid target,
        [CommandArgument] Prototype<EntityPrototype> prototype)
    {
        GetSys<HoleSystem>().AddHole(target,prototype.Value.ID);
    }
}
