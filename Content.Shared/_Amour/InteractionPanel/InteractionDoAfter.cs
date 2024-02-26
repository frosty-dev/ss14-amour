using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.InteractionPanel;

[Serializable, NetSerializable]
public sealed partial class PanelDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public string Prototype;

    public PanelDoAfterEvent(string prototype)
    {
        Prototype = prototype;
    }
}
