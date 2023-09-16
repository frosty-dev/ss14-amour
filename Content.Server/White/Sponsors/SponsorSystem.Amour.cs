using System.Linq;
using Content.Shared.White.Sponsors;
using Robust.Shared.Prototypes;

namespace Content.Server.White.Sponsors;

public sealed class SponsorSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<SponsorInfoLoadEvent>(OnSponsorInfoLoad);
    }

    private void OnSponsorInfoLoad(ref SponsorInfoLoadEvent ev)
    {
        if (_prototype.TryIndex<SponsorSpeciesPrototype>(ev.SponsorInfo.Tier!.Value.ToString(), out var prototype))
        {
            ev.SponsorInfo.AllowedMarkings = ev.SponsorInfo.AllowedMarkings.Concat(prototype.Species).ToArray();
        }
    }
}
