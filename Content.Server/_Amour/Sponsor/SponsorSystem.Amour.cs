using System.Linq;
using Content.Server._White.Sponsors;
using Content.Shared._Amour.Sponsor;
using Content.Shared._White.Sponsors;
using Robust.Shared.Prototypes;

namespace Content.Server._Amour.Sponsor;

public sealed class SponsorSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SponsorInfoLoadEvent>(OnSponsorLoaded);
    }

    private void OnSponsorLoaded(ref SponsorInfoLoadEvent ev)
    {
        if(ev.SponsorInfo.Tier is null || !_prototypeManager.TryIndex<SponsorItemPrototype>(ev.SponsorInfo.Tier.Value.ToString(), out var prototype))
            return;

        ev.SponsorInfo.AllowedMarkings = ev.SponsorInfo.AllowedMarkings.Concat(prototype.Items).ToArray();
    }
}
