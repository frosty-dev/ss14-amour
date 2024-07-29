using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._White.RCD;

[Prototype("rcdCategory")]
public sealed class RCDCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string TooltipBase = "rcd-category-";

    [DataField(required: true)]
    public SpriteSpecifier SpritePath = default!;
}
