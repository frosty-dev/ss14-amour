using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.White.Anus;
using Robust.Shared.Containers;

namespace Content.Server.White.Anus;

public sealed class AnusSystem : SharedAnusSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AnusComponent,EntInsertedIntoContainerMessage>(OnSlotInteraction);
    }

    private void OnSlotInteraction(EntityUid uid, AnusComponent component, EntInsertedIntoContainerMessage args)
    {
        if(args.Container != component.AnusSlot)
            return;

        if (TryComp<ItemComponent>(args.Entity, out var itemComponent) && itemComponent.Size > component.Capacity)
        {
            _popup.PopupEntity(Loc.GetString("anus-blowing"),uid,uid,PopupType.MediumCaution);
            _bloodstream.TryModifyBleedAmount(uid, (itemComponent.Size - component.Capacity)*2);
        }
    }
}
