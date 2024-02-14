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
        base.Initialize();
        InitializeInventory();
    }

    public override void Update(float frameTime)
    {
        UpdateSolution(frameTime);
    }
}
