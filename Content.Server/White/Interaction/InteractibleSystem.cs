using Content.Server.Actions;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.White.ShittyInteraction;
using Content.Shared.White.ShittyInteraction.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using static Content.Shared.White.ShittyInteraction.InteractibleComponent;

namespace Content.Server.White.Interaction;

public sealed class InteractibleSystem : SharedInteractibleSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<InteractionSelectMessage>(OnSelected);
        SubscribeLocalEvent<ExecutionInteractionEvent>(OnInteraction);
        SubscribeLocalEvent<InteractibleComponent, InteractionDoAfterEvent>(OnInteractionDoAfter);
    }

    private void OnInteractionDoAfter(EntityUid uid, InteractibleComponent component, InteractionDoAfterEvent args)
    {
        if(!TryComp<InteractibleComponent>(args.User,out var performerComponent) ||
           !TryComp<InteractibleComponent>(args.Target, out var targetComponent) ||
           !PrototypeManager.TryIndex<InteractionActionPrototype>(args.EventName, out var eventPrototype))
            return;

        performerComponent.IsActive = false;
        targetComponent.IsActive = false;
        _actionBlocker.UpdateCanMove(args.User);
        _actionBlocker.UpdateCanMove(args.Target.Value);

        if(args.Cancelled)
            return;

        if (eventPrototype.EndEvent != null)
        {
            eventPrototype.EndEvent.Performer = args.User;
            eventPrototype.EndEvent.Target = args.Target.Value;
            RaiseLocalEvent((object)eventPrototype.EndEvent);
            RaiseNetworkEvent(eventPrototype.EndEvent);
        }

        if (eventPrototype.EndSound != null)
        {
            _audio.PlayPvs(eventPrototype.EndSound, args.User);
        }

        if (eventPrototype.EndMessages.Count > 0)
        {
            var message = _random.Pick(eventPrototype.EndMessages);
            _chat.TrySendInGameICMessage(args.User,
                Loc.GetString(message, ("target", (MetaData(args.Target.Value).EntityName))), InGameICChatType.Emote, false);
        }

        targetComponent.NextInteractionTime = _timing.CurTime + TimeSpan.FromSeconds(eventPrototype.Timeout);
        performerComponent.NextInteractionTime = _timing.CurTime + TimeSpan.FromSeconds(eventPrototype.Timeout);
    }

    private void OnInteraction(ExecutionInteractionEvent args)
    {
        if (!TryComp<InteractibleComponent>(args.Performer, out var performerComponent) ||
            performerComponent.IsActive ||
            !TryComp<InteractibleComponent>(args.Target, out var targetComponent) || targetComponent.IsActive ||
            !PrototypeManager.TryIndex<InteractionActionPrototype>(args.EventName, out var eventPrototype))
            return;

        if (performerComponent.NextInteractionTime > _timing.CurTime)
        {
            var message = Loc.GetString("interaction-tired");
            if (TryComp<ActorComponent>(args.Performer, out var actor))
                _chatManager.ChatMessageToOne(ChatChannel.Emotes, message, message, EntityUid.Invalid, false,
                    actor.PlayerSession.ConnectedClient);
            return;
        }

        if (eventPrototype.ServerEvent != null)
        {
            eventPrototype.ServerEvent.Performer = args.Performer;
            eventPrototype.ServerEvent.Target = args.Target;

            if (performerComponent.Action != null)
                _actions.RemoveAction(args.Performer, performerComponent.Action);

            var ev = eventPrototype.ServerEvent;
            RaiseLocalEvent((object) ev);

            if (ev.Cancelled)
            {
                ev.Uncancel();
                return;
            }

            RaiseNetworkEvent(ev);
        }

        if (eventPrototype.IsCloseInteraction)
        {
            _actionBlocker.UpdateCanMove(args.Performer);
            _actionBlocker.UpdateCanMove(args.Target);
            _transform.SetCoordinates(args.Performer, Transform(args.Target).Coordinates);
        }

        if (eventPrototype.InteractionTime > 0)
        {
            performerComponent.IsActive = true;
            targetComponent.IsActive = true;

            var doAfterArgs = new DoAfterArgs(args.Performer, TimeSpan.FromSeconds(eventPrototype.InteractionTime),
                new InteractionDoAfterEvent(args.EventName),
                args.Performer, args.Target)
            {
                BreakOnHandChange = false,
                BreakOnUserMove = true,
                BreakOnTargetMove = true
            };

            _doAfter.TryStartDoAfter(doAfterArgs);
        }

        if (eventPrototype.Messages.Count > 0)
        {
            var message = _random.Pick(eventPrototype.Messages);
            _chat.TrySendInGameICMessage(args.Performer,
                Loc.GetString(message, ("target", (MetaData(args.Target).EntityName))), InGameICChatType.Emote, false);
        }

        if (eventPrototype.StartSound != null)
        {
            _audio.PlayPvs(eventPrototype.StartSound, args.Performer);
        }


}

    private void OnSelected(InteractionSelectMessage ev, EntitySessionEventArgs args)
    {
        var playerUid = args.SenderSession.AttachedEntity;
        if(!playerUid.HasValue || !TryComp<InteractibleComponent>(playerUid.Value,out var component)
                               || !component.AvailableInteractions.Contains(ev.SelectedInteraction)
           || !PrototypeManager.TryIndex<InteractionActionPrototype>(ev.SelectedInteraction, out var action)
                               || !TryComp<ActionsComponent>(playerUid,out var actionsComponent))
            return;

        if (component.NextInteractionTime > _timing.CurTime)
        {
            var message = Loc.GetString("interaction-tired");
            if(TryComp<ActorComponent>(playerUid,out var actor))
                _chatManager.ChatMessageToOne(ChatChannel.Emotes,message,message,EntityUid.Invalid, false, actor.PlayerSession.ConnectedClient);
            return;
        }

        if (component.Action != null)
            _actions.RemoveAction(playerUid.Value, component.Action, actionsComponent);

        component.Action = new EntityTargetAction(action)
        {
            Event = new ExecutionInteractionEvent(ev.SelectedInteraction)
        };


        _actions.AddAction(playerUid.Value,component.Action,null,actionsComponent);
        _actions.SetToggled(component.Action,true);
    }
}
