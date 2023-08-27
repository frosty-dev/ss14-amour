using Robust.Shared.Containers;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.Anus;

[RegisterComponent]
public sealed class AnusComponent : Component
{
    [ViewVariables] public ContainerSlot AnusSlot = default!;

    [DataField("capacity")] public float Capacity = 5;

    public static string SlotName = "anus";

    [DataField("nextMoan", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextMoanTime;

    [ViewVariables(VVAccess.ReadWrite)] public TimeSpan MoanRate = TimeSpan.FromSeconds(1);
}
