namespace Content.Shared.White.Leash;

[RegisterComponent]
public sealed class LeashComponent : Component
{
    [ViewVariables] public int LeashedCount = 0;
    [DataField("max")] public int LeashedMax = 1;

    [ViewVariables] public List<EntityUid> LeashedUid = new();

    [ViewVariables] public EntityUid? PickupedUid;
}
