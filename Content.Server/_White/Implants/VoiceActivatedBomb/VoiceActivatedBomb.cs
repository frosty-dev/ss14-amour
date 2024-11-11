using Content.Shared._White.Implants.VoiceActivatedBomb;
using Content.Shared.Implants.Components;
using Content.Server.Explosion.Components;
using Content.Server.Speech.Components;
using Content.Shared.Popups;
using Content.Shared.Implants;

namespace Content.Server._White.Implants.VoiceActivatedBomb;

public sealed class VoiceActivatedBombSystem : SharedVoiceActivatedBombSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SubdermalImplantComponent, InsertVoiceActivatedBombEvent>(OnTryInsertVoiceActivatedBombServer);
        SubscribeLocalEvent<SubdermalImplantComponent, ImplanterUsed>(OnVoiceActivatedBombInserted);
    }

    private void OnTryInsertVoiceActivatedBombServer(Entity<SubdermalImplantComponent> ent, ref InsertVoiceActivatedBombEvent args)
    {
        if (!TryComp<TriggerOnVoiceComponent>(args.Implanter, out var implanterTrigger))
            return;

        if (implanterTrigger.KeyPhrase == null)
        {
            // TODO find some way to make it look good
            // Right now it's overlaps with implanter-component-implant-failed popup
            //var message = Loc.GetString("voice-activated-bomb-no-key-phrase");
            //_popup.PopupEntity(message, args.Implanter, args.User);
            args.Cancel();
            return;
        }
    }

    private void OnVoiceActivatedBombInserted(Entity<SubdermalImplantComponent> ent, ref ImplanterUsed args)
    {
        if (!Tag.HasTag(args.Implanter, VoiceActivatedBombTag))
            return;

        if (!TryComp<TriggerOnVoiceComponent>(args.Implanter, out var implanterTrigger))
            return;

        if (!TryComp<TriggerOnVoiceComponent>(args.Implant, out var implantTrigger))
            return;

        implantTrigger.KeyPhrase = implanterTrigger.KeyPhrase;
        EnsureComp<ActiveListenerComponent>(args.Implant);
        RemComp<TriggerOnVoiceComponent>(args.Implanter);
        Tag.RemoveTag(args.Implanter, VoiceActivatedBombTag);
    }
}
