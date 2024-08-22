using Content.Server._White.AspectsSystem.Base;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.VendingMachines;

namespace Content.Server._Honk.Aspects;

public sealed class DeleteVendingMachineAspect : AspectSystem<DeleteVendingMachineComponent>
{
    [Dependency] private readonly EntityManager _entityManager = default!;

    protected override void Started(EntityUid uid, DeleteVendingMachineComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        foreach (var entity in _entityManager.EntityQuery<VendingMachineComponent>())
        {
            _entityManager.DeleteEntity(entity.Owner);
        }
    }
}
