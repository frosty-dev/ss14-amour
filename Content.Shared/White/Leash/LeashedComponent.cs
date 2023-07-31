namespace Content.Shared.White.Leash;

[RegisterComponent]
public sealed class LeashedComponent : Component
{
    [ViewVariables]
    public EntityUid LeashUid;

    public static string LeashJoint = "leashJoint";
}
