using Robust.Shared.Prototypes;

namespace Content.Shared.White.Sponsors;

[Prototype("sponsorSpecies")]
public sealed class SponsorSpeciesPrototype : IPrototype
{
    //Сюда тиры да
    [IdDataField] public string ID { get; private set; } =  default!;
    //[DataField("tier")] public int Tier = 1; //1 митиорка 2 котик а дальше не ебу
    [DataField("species")] public List<string> Species = new();
}
