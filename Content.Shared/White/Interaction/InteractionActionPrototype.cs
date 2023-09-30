using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.White.Interaction;

[Prototype("interactionTargetAction")]
public sealed class InteractionActionPrototype : EntityTargetAction, IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("serverEvent", serverOnly: true)]
    public EntityTargetActionEvent? ServerEvent
    {
        get => Event;
        set => Event = value;
    }

    [DataField("interactionTime",customTypeSerializer: typeof(TimeOffsetSerializer) )]
    public TimeSpan InteractionTime = TimeSpan.Zero;
}
