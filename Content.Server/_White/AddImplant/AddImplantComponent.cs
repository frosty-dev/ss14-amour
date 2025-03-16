using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server._White.AddImplant;

/// <summary>
/// WD.
/// </summary>
[RegisterComponent]
public sealed partial class AddImplantComponent : Component
{
    [DataField("implants", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<EntityPrototype>))]
    public HashSet<String> Implants { get; private set; } = new();
}
