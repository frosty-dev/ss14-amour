using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.InteractionPanel;

[Prototype("interaction")]
public sealed partial class InteractionPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [ViewVariables] public PrototypeLayerData? Icon;

    [DataField] public SoundSpecifier? BeginningSound;
    [DataField] public SoundSpecifier? EndingSound;

    [DataField] public List<string> BeginningMessages = new();
    [DataField] public List<string> EndingMessages = new();

    [DataField] public List<IInteractionCheck> Checks = new();

    [DataField] public TimeSpan EndTime = TimeSpan.Zero;
    [DataField] public TimeSpan Timeout = TimeSpan.FromSeconds(3);
}

