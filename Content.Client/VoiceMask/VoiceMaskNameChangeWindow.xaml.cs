using Content.Shared._White.TTS;
using Content.Client.UserInterface.Controls;
using Content.Shared.Speech;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.VoiceMask;

[GenerateTypedNameReferences]
public sealed partial class VoiceMaskNameChangeWindow : FancyWindow
{
    public Action<string>? OnNameChange;
    public Action<string>? OnVoiceChange;
    public Action<string?>? OnVerbChange;

    private readonly List<TTSVoicePrototype> _voices = new(); // TTS
    private readonly List<(string, string)> _verbs = new();

    private string? _verb;

    public VoiceMaskNameChangeWindow(IPrototypeManager proto)
    {
        RobustXamlLoader.Load(this);

        NameSelectorSet.OnPressed += _ =>
        {
            OnNameChange?.Invoke(NameSelector.Text);
        };

        VoiceSelector.OnItemSelected += args =>
        {
            VoiceSelector.SelectId(args.Id);
            if (VoiceSelector.SelectedMetadata != null)
                OnVoiceChange!((string) VoiceSelector.SelectedMetadata);
        };

        SpeechVerbSelector.OnItemSelected += args =>
        {
            OnVerbChange?.Invoke((string?) args.Button.GetItemMetadata(args.Id));
            SpeechVerbSelector.SelectId(args.Id);
        };

        ReloadVoices(proto);
        AddVoices();

        ReloadVerbs(proto);
        AddVerbs();
    }

    private void ReloadVoices(IPrototypeManager proto)
    {
        foreach (var voice in proto.EnumeratePrototypes<TTSVoicePrototype>())
        {
            if (voice.RoundStart)
            {
                _voices.Add(voice);
            }
        }

        _voices.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }

    private void AddVoices()
    {
        VoiceSelector.Clear();
        for (var i = 0; i < _voices.Count; i++)
        {
            var name = Loc.GetString(_voices[i].Name);
            VoiceSelector.AddItem(name);
            VoiceSelector.SetItemMetadata(i, _voices[i].ID);
        }
    }

    private void ReloadVerbs(IPrototypeManager proto)
    {
        foreach (var verb in proto.EnumeratePrototypes<SpeechVerbPrototype>())
        {
            _verbs.Add((Loc.GetString(verb.Name), verb.ID));
        }

        _verbs.Sort((a, b) => string.Compare(a.Item1, b.Item1, StringComparison.Ordinal));
    }

    private void AddVerbs()
    {
        SpeechVerbSelector.Clear();

        AddVerb(Loc.GetString("chat-speech-verb-name-none"), null);
        foreach (var (name, id) in _verbs)
        {
            AddVerb(name, id);
        }
    }

    private void AddVerb(string name, string? verb)
    {
        var id = SpeechVerbSelector.ItemCount;
        SpeechVerbSelector.AddItem(name);
        if (verb != null)
            SpeechVerbSelector.SetItemMetadata(id, verb);

        if (verb == _verb)
            SpeechVerbSelector.SelectId(id);
    }

    public void UpdateState(string name, string voice, string? verb)
    {
        NameSelector.Text = name;
        _verb = verb;
        var voiceIdx = _voices.FindIndex(v => v.ID == voice);
        if (voiceIdx != -1)
            VoiceSelector.Select(voiceIdx);

        for (var id = 0; id < SpeechVerbSelector.ItemCount; id++)
        {
            if (Equals(verb, SpeechVerbSelector.GetItemMetadata(id)))
            {
                SpeechVerbSelector.SelectId(id);
                break;
            }
        }
    }
}