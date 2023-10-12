using Robust.Shared.Prototypes;

namespace Content.Shared.White.RolePlayThink;

[Prototype("rolePlayThink")]
public sealed class RoleplayThinkPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField("name",required:true)]
    public string Name = default!;

    [DataField("description", required:true)]
    public string Description = default!;
}
