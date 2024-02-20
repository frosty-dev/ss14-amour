using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.HumanoidAppearanceExtension;

public record struct HumanoidAppearanceLoadingEvent(
    Entity<HumanoidAppearanceComponent> Entity,
    HumanoidCharacterProfile Profile);

public record struct HumanoidAppearanceLoadedEvent(
    Entity<HumanoidAppearanceComponent> Entity,
    HumanoidCharacterProfile Profile);
