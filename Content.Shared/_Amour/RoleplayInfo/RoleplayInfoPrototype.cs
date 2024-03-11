using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.RoleplayInfo;

[Prototype]
public sealed class RoleplayInfoPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
}
