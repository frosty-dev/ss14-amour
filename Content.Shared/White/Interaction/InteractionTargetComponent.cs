namespace Content.Shared.White.Interaction;

[RegisterComponent]
public sealed class InteractionTargetComponent : Component
{
    [DataField("isActive"), AutoNetworkedField]
    public bool IsActive;
}
