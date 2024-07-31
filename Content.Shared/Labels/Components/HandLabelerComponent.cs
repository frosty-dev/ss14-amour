using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Labels.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class HandLabelerComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField] public string AssignedLabel { get; set; } = string.Empty;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField] public int MaxLabelChars { get; set; } = 50;

        [DataField] public EntityWhitelist Whitelist = new();
    }
}
