using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Shared.StatusIcon;

/// <summary>
/// A data structure that holds relevant
/// information for status icons.
/// </summary>
[Virtual, DataDefinition]
public partial class StatusIconData : IComparable<StatusIconData>
{
    /// <summary>
    /// The icon that's displayed on the entity.
    /// </summary>
    [DataField("icon", required: true)]
    public SpriteSpecifier Icon = default!;

    /// <summary>
    /// A priority for the order in which the icons will be displayed.
    /// </summary>
    [DataField("priority")]
    public int Priority = 10;

    /// <summary>
    /// A preference for where the icon will be displayed. None | Left | Right
    /// </summary>
    [DataField("locationPreference")]
    public StatusIconLocationPreference LocationPreference = StatusIconLocationPreference.None;

    public int CompareTo(StatusIconData? other)
    {
        return Priority.CompareTo(other?.Priority ?? int.MaxValue);
    }

    /// <summary>
    /// The layer the icon is displayed on. Mod is drawn above Base. Base | Mod
    /// </summary>
    [DataField]
    public StatusIconLayer Layer = StatusIconLayer.Base;

    /// <summary>
    /// Offset of the status icon, up and down only.
    /// </summary>
    [DataField]
    public int Offset;

    /// <summary>
    /// Sets if the icon should be rendered with or without the effect of lighting.
    /// </summary>
    [DataField]
    public bool IsShaded;
}

/// <summary>
/// <see cref="StatusIconData"/> but in new convenient prototype form!
/// </summary>
[Prototype("statusIcon")]
public sealed partial class StatusIconPrototype : StatusIconData, IPrototype, IInheritingPrototype
{
    /// <inheritdoc />
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<StatusIconPrototype>))]
    public string[]? Parents { get; }

    /// <inheritdoc />
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }

    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;
}

[Serializable, NetSerializable]
public enum StatusIconLocationPreference : byte
{
    None,
    Left,
    Right,
}

public enum StatusIconLayer : byte
{
    Base,
    Mod,
}
