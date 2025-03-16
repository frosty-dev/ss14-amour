
using Content.Server._White.ContractorIDCard;
using Content.Server.Access.Systems;
using Content.Server.Forensics;
using Content.Server.PDA;
using Content.Shared.Access.Components;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Robust.Server.Containers;
using Robust.Shared.Timing;

namespace Content.Server._White.FillIDCard;

public sealed class FillIDCardSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly PdaSystem _pdaSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly IdCardSystem _idCardSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FillIDCardComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<FillIDCardComponent> ent, ref MapInitEvent args)
    {
        Timer.Spawn(15000, () => ItDoBeDoing(ent)); // 15 seconds and yes this is bad // TODO Make it less dogshit
    }

    private void ItDoBeDoing(Entity<FillIDCardComponent> ent)
    {
        if (!TryComp<MetaDataComponent>(ent.Owner, out var targetMeta))
        {
            RemComp<FillIDCardComponent>(ent.Owner);
            return;
        }

        if (!_inventorySystem.TryGetSlotEntity(ent.Owner, "id", out var idcardSlot))
        {
            RemComp<FillIDCardComponent>(ent.Owner);
            return;
        }

        if (TryComp<PdaComponent>(idcardSlot, out var pda))
        {
            _pdaSystem.SetOwnerName((EntityUid) idcardSlot, pda, targetMeta.EntityName);

            _containerSystem.TryGetContainer((EntityUid) idcardSlot, "PDA-id", out var container);

            if (container != null)
            {
                var idCardInPda = container.ContainedEntities[0];
                _idCardSystem.TryChangeFullName(idCardInPda, targetMeta.EntityName);
            }
        }
        else if (HasComp<IdCardComponent>(idcardSlot))
        {
            _idCardSystem.TryChangeFullName((EntityUid) idcardSlot, targetMeta.EntityName);
            if (ent.Comp.IsContractor)
            {
                EnsureComp<ContractorIDCardComponent>((EntityUid) idcardSlot, out var comp);
                if (TryComp<FingerprintComponent>(ent.Owner, out var fingerprintComponent) && TryComp<DnaComponent>(ent.Owner, out var dnaComponent))
                    comp.Details = $"На карте имеется следующая информация:\nВладелец карты: {targetMeta.EntityName}\nОтпечаток пальцев владельца: {fingerprintComponent.Fingerprint}\nДНК владельца: {dnaComponent.DNA}";
            }
        }

        RemComp<FillIDCardComponent>(ent.Owner);
    }
}
