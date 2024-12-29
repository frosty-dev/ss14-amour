using Content.Server.Chat.Systems;
using Content.Server.VoiceMask;
using Content.Shared.Interaction.Components;
using Content.Server._White.HopSpeaker;

namespace Content.Server.Chat;

public sealed class FunnyFontsChatSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpeechTransformedEvent>(OnEntitySpeak);
    }

    private void OnEntitySpeak(SpeechTransformedEvent ev)
    {
        if (TryComp(ev.Sender, out VoiceMaskComponent? mask) && mask.Enabled) return;

        if (TryComp<ClumsyComponent>(ev.Sender, out _))
        {
            ev.Message = $"[font=\"ComicSansMS\"]{ev.Message}[/font]";
        }
        // WD EDIT START
        if (TryComp<PapersSpeechComponent>(ev.Sender, out _))
        {
            ev.Message = $"[font=\"BureaucracyPixel\"]{ev.Message}[/font]";
        }
        // WD EDIT END
    }
}
