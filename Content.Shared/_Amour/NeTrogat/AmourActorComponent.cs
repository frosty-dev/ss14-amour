using Robust.Shared.Player;

namespace Content.Shared._Amour.NeTrogat;

[RegisterComponent]
public sealed partial class AmourActorComponent : Component
{
    [ViewVariables]
    public ICommonSession PlayerSession { get; internal set; } = default!;
}
