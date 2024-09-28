using Robust.Shared.Audio;

namespace Content.Server._White._Engi.PacifiedOnChaplainAction
{
    /// <summary>
    /// Adds verb for chaplain to pacify entity.
    /// WD Engi Exclusive.
    /// </summary>
    [RegisterComponent]
    public sealed partial class PacifiedOnChaplainActionComponent : Component
    {
        [DataField]
        public SoundSpecifier ActionSound = new SoundPathSpecifier("/Audio/Effects/holy.ogg");
    }
}
