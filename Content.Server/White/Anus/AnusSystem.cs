using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.White.Anus;
using Content.Shared.White.Dildo;
using Content.Shared.White.Mood;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server.White.Anus;

public sealed class AnusSystem : SharedAnusSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AnusComponent,EntInsertedIntoContainerMessage>(OnInsert);
    }

    private void OnInsert(EntityUid uid, AnusComponent component, EntInsertedIntoContainerMessage args)
    {
        if(args.Container != component.AnusSlot)
            return;

        if (TryComp<ItemComponent>(args.Entity, out var itemComponent) && itemComponent.Size > component.Capacity)
        {
            _popup.PopupEntity(Loc.GetString("anus-blowing"),uid,uid,PopupType.MediumCaution);
            _bloodstream.TryModifyBleedAmount(uid, (itemComponent.Size)*2);
            _chat.TryEmoteWithChat(uid,"Scream");
            RaiseLocalEvent(uid, new MoodEffectEvent("AnusTorn"));
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("anus-inserting"),uid,uid,PopupType.Medium);
            _chat.TryEmoteWithChat(uid,"Moan");
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AnusComponent>();

        while (query.MoveNext(out var uid,out var component))
        {
            if (Timing.CurTime < component.NextMoanTime)
                return;
            component.NextMoanTime += component.MoanRate;

            if((!component.AnusSlot.ContainedEntity.HasValue ||
               !TryComp<DildoComponent>(component.AnusSlot.ContainedEntity.Value,out var dildoComponent) ||
               !dildoComponent.IsVibrating) && !HasComp<MoanComponent>(uid))
                continue;

            _chat.TryEmoteWithChat(uid,"Moan");
            RaiseLocalEvent(uid, new MoodEffectEvent("Dildo"));
        }
    }
}
