using Content.Shared.Inventory.Events;
using Content.Shared._White._Engi.BucketHelmet;

namespace Content.Server._White._Engi.BucketHelmet;

/// <summary>
/// WD.
/// This handles placemet of PreventStrippingFromEarsComponent when bucket helmet is in use.
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
