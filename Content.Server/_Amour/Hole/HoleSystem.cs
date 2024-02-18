using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared._Amour.Hole;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

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
