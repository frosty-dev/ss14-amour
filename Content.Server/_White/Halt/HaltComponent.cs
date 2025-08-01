using Robust.Shared.Audio;

namespace Content.Server._White.Halt;

[RegisterComponent]
public sealed partial class HaltComponent : Component
{
    [DataField("color"), ViewVariables(VVAccess.ReadWrite)]
    public string ChatColor { get; private set; } = Color.Red.ToHex();

    [DataField("locale"), ViewVariables(VVAccess.ReadWrite)]
    public string ChatLoc { get; private set; } = "chat-manager-entity-say-hailer-wrap-message";

    [DataField("actionEntity")] public EntityUid? ActionEntity;

    public readonly Dictionary<string, SoundSpecifier> PhraseToSoundMap = new()
    {
        ["halt-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/halt.ogg"),
        ["bobby-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/bobby.ogg"),
        ["compliance-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/compliance.ogg"),
        ["justice-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/justice.ogg"),
        ["running-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/running.ogg"),
        ["dontmove-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/dontmove.ogg"),
        ["floor-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/floor.ogg"),
        ["robocop-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/robocop.ogg"),
        ["freeze-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/freeze.ogg"),
        ["imperial-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/imperial.ogg"),
        ["bash-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/bash.ogg"),
        ["harry-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/harry.ogg"),
        ["asshole-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/asshole.ogg"),
        ["stfu-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/stfu.ogg"),
        ["shutup-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/shutup.ogg"),
        ["dredd-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/dredd.ogg")
    };
}
