using Content.Shared.Inventory.Events;
using Content.Shared._White.BucketHelmet;

namespace Content.Server._White.BucketHelmet;

/// <summary>
/// This handles placemet of PreventStrippingFromEarsComponent when bucket helmet is in use.
/// WD Engi Exclusive.
/// </summary>
public sealed class BucketHelmetSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<BucketHelmetComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<BucketHelmetComponent, GotUnequippedEvent>(OnGotUnequipped);
    }


    public void OnGotUnequipped(EntityUid uid, BucketHelmetComponent component, GotUnequippedEvent args)
    {
        RemComp<PreventStrippingFromEarsComponent>(args.Equipee);
    }

    public void OnGotEquipped(EntityUid uid, BucketHelmetComponent component, GotEquippedEvent args)
    {
        if (args.Slot == "head")
            EnsureComp<PreventStrippingFromEarsComponent>(args.Equipee);
    }

}
