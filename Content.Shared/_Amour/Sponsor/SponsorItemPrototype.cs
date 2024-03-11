using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.Sponsor;

[Prototype]
public sealed class SponsorItemPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [DataField] public List<string> Items = new();
}
