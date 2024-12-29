using Robust.Shared.Audio;

namespace Content.Server._White.HopSpeaker;

[RegisterComponent]
public sealed partial class HopSpeakerComponent : Component
{
    [DataField("delay")]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan DelayEnd = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/White/Effects/dynamicNEXT_.ogg");
}

