using System.Threading;

namespace Content.Server._White.Carrying;

[RegisterComponent]
public sealed partial class CarriableComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan DoAfterLength = TimeSpan.FromSeconds(4);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float WalkModifier = 0.7f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float SprintModifier = 0.7f;

    /// <summary>
    ///     Number of free hands required
    ///     to carry the entity
    /// </summary>
    [DataField]
    public int FreeHandsRequired = 2;

    public CancellationTokenSource? CancelToken;
}
