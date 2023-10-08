using System.Linq;
using Content.Server.Actions;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Interaction;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Strip;
using Content.Shared.Verbs;
using Content.Shared.White.ShittyInteraction;
using Content.Shared.White.ShittyInteraction.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.White.Interaction;

public sealed class InteractibleSystem : SharedInteractibleSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly RotateToFaceSystem _face = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InteractibleComponent, InteractionDoAfterEvent>(OnInteractionDoAfter);
        SubscribeLocalEvent<InteractibleComponent, InteractionSelectedMessage>(OnInteractionSelected);
        SubscribeLocalEvent<InteractibleComponent, DragDropDraggedEvent>(OnDrag, new []{ typeof(SharedStrippableSystem) });
        SubscribeLocalEvent<InteractibleComponent, GetVerbsEvent<Verb>>(OnVerb);

        SubscribeLocalEvent<InteractibleComponent, SexChangedEvent>(OnSexChanged);
        SubscribeLocalEvent<InteractibleComponent, EntInsertedIntoContainerMessage>(OnUpdate);
        SubscribeLocalEvent<InteractibleComponent, EntRemovedFromContainerMessage>(OnUpdate);
    }

    private void OnVerb(EntityUid uid, InteractibleComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Target == args.User)
            return;

        Verb verb = new()
        {
            Text = Loc.GetString("interaction-verb-get-data-text"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/group.svg.192dpi.png")),
            Act = () => OpenInteractionMenu(args.User,args.Target),
        };
        args.Verbs.Add(verb);
    }

    private void OnUpdate(EntityUid uid, InteractibleComponent component, EntityEventArgs args)
    {
        OnUpdateRequired(uid,component);
    }

    private void OnSexChanged(EntityUid uid, InteractibleComponent component, SexChangedEvent args)
    {
        OnUpdateRequired(uid,component);
    }

    private void OnUpdateRequired(EntityUid uid, InteractibleComponent component)
    {
        if(TryComp<ServerUserInterfaceComponent>(uid,out var serverUserInterfaceComponent))
            UpdateUserInterface(uid,serverUserInterfaceComponent);

        if (TryComp<ActorComponent>(uid, out var actorComponent))
        {
            var buiList = _userInterfaceSystem.GetAllUIsForSession(actorComponent.PlayerSession);
            if(buiList == null)
                return;

            foreach (var bui in buiList)
            {
                UpdateUserInterface(bui.Owner);
            }
        }
    }

    private void OnDrag(EntityUid uid, InteractibleComponent component,ref DragDropDraggedEvent args)
    {
        if (args.Handled || args.Target != args.User || !HasComp<InteractibleComponent>(args.User))
            return;

        OpenInteractionMenu(args.User,uid);
        args.Handled = true;
    }

    private void OnInteractionSelected(EntityUid uid, InteractibleComponent targetComponent, InteractionSelectedMessage args)
    {
        var playerUid = args.Session.AttachedEntity;
        if(!playerUid.HasValue
           || !PrototypeManager.TryIndex<InteractionActionPrototype>(args.SelectedInteraction, out var interaction))
            return;

        Interact(playerUid.Value,uid,new InteractionAction(interaction));
    }

    private void OnInteractionDoAfter(EntityUid uid, InteractibleComponent component, InteractionDoAfterEvent args)
    {
        if(!TryComp<InteractibleComponent>(args.User,out var performerComponent) ||
           !TryComp<InteractibleComponent>(args.Target, out var targetComponent))
            return;

        var interactionAction = args.Action;

        performerComponent.IsActive = false;
        targetComponent.IsActive = false;
        _actionBlocker.UpdateCanMove(args.User);
        _actionBlocker.UpdateCanMove(args.Target.Value);

        if (interactionAction.EndEvent != null)
        {
            interactionAction.EndEvent.Performer = args.User;
            interactionAction.EndEvent.Target = args.Target.Value;
            if (args.Cancelled)
                interactionAction.EndEvent.Cancel();

            RaiseLocalEvent((object)interactionAction.EndEvent);
            RaiseNetworkEvent(interactionAction.EndEvent);

            interactionAction.EndEvent.Uncancel();
        }

        if(args.Cancelled)
            return;

        if (interactionAction.EndSound != null)
        {
            _audio.PlayPvs(interactionAction.EndSound, args.User);
        }

        if (interactionAction.EndMessages.Count > 0)
        {
            var gender = GetGender(args.User);
            var message = _random.Pick(interactionAction.EndMessages);
            _chat.TrySendInGameICMessage(args.User, Loc.GetString(message, ("target",
                    (MetaData(args.Target.Value).EntityName)),("gender",GenderToString(gender))), InGameICChatType.Emote, false);
        }

        targetComponent.NextInteractionTime = Timing.CurTime + TimeSpan.FromSeconds(interactionAction.Timeout);
        performerComponent.NextInteractionTime = Timing.CurTime + TimeSpan.FromSeconds(interactionAction.Timeout);
    }

    public void Interact(EntityUid uid, EntityUid target,InteractionAction interactionAction,
        InteractibleComponent? targetComponent = null,InteractibleComponent? performerComponent = null)
    {
        if(!Resolve(target,ref targetComponent) || !Resolve(uid,ref performerComponent))
            return;

        if (TryComp<ActorComponent>(uid, out var actor))
            _userInterfaceSystem.TryClose(target, InteractionUiKey.Key, actor.PlayerSession);

        if(!_actions.ValidateEntityTarget(uid,target,interactionAction))
        {
            SpellSomeShit(uid,"interaction-far");
            return;
        }

        if (!IsInteractionAvailable(uid, target, interactionAction))
        {
            SpellSomeShit(uid,"interaction-dress");
            return;
        }

        if (targetComponent.NextInteractionTime > Timing.CurTime)
        {
            SpellSomeShit(uid,"interaction-tired");
            return;
        }

        _face.TryFaceCoordinates(uid, _transform.GetWorldPosition(target));

        if (interactionAction.StartEvent != null)
        {
            interactionAction.StartEvent.Performer = uid;
            interactionAction.StartEvent.Target = target;

            var ev = interactionAction.StartEvent;
            RaiseLocalEvent((object) ev);

            if (ev.Cancelled)
            {
                ev.Uncancel();
                return;
            }

            RaiseNetworkEvent(ev);
        }

        if (interactionAction.IsCloseInteraction)
            _transform.SetCoordinates(uid, Transform(target).Coordinates);

        if (interactionAction.InteractionTime > 0)
        {
            performerComponent.IsActive = true;
            targetComponent.IsActive = true;

            if (interactionAction.IsCloseInteraction)
            {
                _actionBlocker.UpdateCanMove(uid);
                _actionBlocker.UpdateCanMove(target);
            }

            var doAfterArgs = new DoAfterArgs(uid, TimeSpan.FromSeconds(interactionAction.InteractionTime),
                new InteractionDoAfterEvent(interactionAction),
                uid, target)
            {
                BreakOnHandChange = false,
                BreakOnUserMove = true,
                BreakOnTargetMove = true
            };

            _doAfter.TryStartDoAfter(doAfterArgs);
        }

        if (interactionAction.Messages.Count > 0)
        {
            var gender = GetGender(uid);
            var message = _random.Pick(interactionAction.Messages);
            _chat.TrySendInGameICMessage(uid,
                Loc.GetString(message, ("target", (MetaData(target).EntityName)),("gender",GenderToString(gender))), InGameICChatType.Emote, false);
        }

        if (interactionAction.StartSound != null)
        {
            _audio.PlayPvs(interactionAction.StartSound, uid);
        }
    }

    public void UpdateUserInterface(EntityUid target,ServerUserInterfaceComponent? component = null)
    {
        if(!Resolve(target,ref component))
            return;

        if(!component.Interfaces.TryGetValue(InteractionUiKey.Key, out var interactionsSessions))
            return;

        foreach (var session in interactionsSessions.SubscribedSessions)
        {
            _userInterfaceSystem.TrySendUiMessage(target, InteractionUiKey.Key,
                new InteractionAvailableMessage(
                    GetAvailableInteractions(session.AttachedEntity!.Value, target).ToList()
                    ), session);
        }
    }

    public void OpenInteractionMenu(EntityUid uid, EntityUid target, InteractibleComponent? component = null)
    {
        if(!Resolve(target,ref component))
            return;

        if (TryComp<ActorComponent>(uid, out var actor))
        {
            if (_userInterfaceSystem.SessionHasOpenUi(target, InteractionUiKey.Key, actor.PlayerSession))
                return;
            _userInterfaceSystem.TryOpen(target, InteractionUiKey.Key, actor.PlayerSession);
        }

        UpdateUserInterface(target);
    }

    protected void SpellSomeShit(EntityUid uid, string message)
    {
        var gender = GetGender(uid);

        message = Loc.GetString(message,("gender",GenderToString(gender)));
        if(TryComp<ActorComponent>(uid,out var actor))
        {
            _chatManager.ChatMessageToOne(ChatChannel.Emotes,message,message,EntityUid.Invalid,
                false, actor.PlayerSession.ConnectedClient);
        }
    }
}
