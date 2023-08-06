using Robust.Shared.Containers;

namespace Content.Shared.White.Anus;

[RegisterComponent]
public sealed class AnusComponent : Component
{
    [ViewVariables] public ContainerSlot AnusSlot = default!;

    [DataField("capacity")] public float Capacity = 5;

    public static string SlotName = "anus";
}
