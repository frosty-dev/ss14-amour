using Content.Shared._Amour.InteractionPanel.Actions;
using Content.Shared._Amour.InteractionPanel.Checks;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.InteractionPanel;

[Prototype("interaction")]
public sealed partial class InteractionPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [ViewVariables] public PrototypeLayerData? Icon;

    [DataField] public SoundSpecifier? BeginningSound;
    [DataField] public SoundSpecifier? EndingSound;

    [DataField] public List<string> PreBeginMessages = new();
    [DataField] public List<string> BeginningMessages = new();
    [DataField] public List<string> EndingMessages = new();

    [DataField] public List<IInteractionCheck> Checks = new();
    [DataField] public List<IInteractionAction> BeginningActions = new();
    [DataField] public List<IInteractionAction> EndingActions = new();

    [DataField] public TimeSpan EndTime = TimeSpan.Zero;
    [DataField] public TimeSpan Timeout = TimeSpan.FromSeconds(3);
    [DataField] public TimeSpan BeginningTimeout = TimeSpan.Zero;
    [DataField] public ProtoId<InteractionGroupPrototype> Group = "Safe";
}

[Prototype("interactionList")]
public sealed class InteractionListPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [DataField] public List<ProtoId<InteractionPrototype>> Prototypes = new List<ProtoId<InteractionPrototype>>();
}
