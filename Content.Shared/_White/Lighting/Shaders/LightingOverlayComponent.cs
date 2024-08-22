using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using static Robust.Shared.Utility.SpriteSpecifier;

namespace Content.Shared._White.Lighting.Shaders;

/// <summary>
/// This is used for LightOverlay
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LightingOverlayComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool? Enabled;

    [DataField]
    public SpriteSpecifier Sprite = new Texture(new ResPath("Effects/LightMasks/lightmask_lamp.png"));

    [DataField]
    public float Offsetx = -0.5f;

    [DataField]
    public float Offsety = 0.5f;

    [DataField]
    public Color? Color;
}
