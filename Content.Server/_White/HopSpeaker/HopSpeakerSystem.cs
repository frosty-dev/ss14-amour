using Content.Server.Explosion.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Power.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._White.HopSpeaker;

public sealed partial class HopSpeakerSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HopSpeakerComponent, TriggerEvent>(HandleChatOnTrigger);
    }

    private void HandleChatOnTrigger(EntityUid uid, HopSpeakerComponent component, TriggerEvent args)
    {
        if (component.DelayEnd > _gameTiming.CurTime)
            return;

        if (!TryComp<ApcPowerReceiverComponent>(uid, out var receiverComponent) || !receiverComponent.Powered)
            return;

        _chat.TrySendInGameICMessage(uid, Loc.GetString("hopspeaker-next"), InGameICChatType.Speak,
            ChatTransmitRange.Normal);
        _audio.PlayEntity(component.Sound, Filter.Local(), uid, false, AudioParams.Default.WithVolume(-2f));

        component.DelayEnd = _gameTiming.CurTime + component.Delay;
    }
}
