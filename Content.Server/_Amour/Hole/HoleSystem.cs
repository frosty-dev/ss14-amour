using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared._Amour.Hole;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._Amour.Hole;

public sealed partial class HoleSystem : SharedHoleSystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        InitializeContainer();
        InitializeInventory();
        SubscribeLocalEvent<HoleComponent,EntGotInsertedIntoContainerMessage>(OnInsert);
    }

    private void OnInsert(EntityUid uid, HoleComponent component, EntGotInsertedIntoContainerMessage args)
    {
        component.Parent = GetNetEntity(args.Container.Owner);
    }

    public override void Update(float frameTime)
    {
        UpdateSolution(frameTime);
    }
}
