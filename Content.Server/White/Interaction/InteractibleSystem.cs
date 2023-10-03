using System.Reflection;
using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Events;
using Content.Shared.White.ShittyInteraction;
using Content.Shared.White.ShittyInteraction.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using InteractibleComponent = Content.Shared.White.ShittyInteraction.InteractibleComponent;

namespace Content.Server.White.Interaction;

public sealed class InteractibleSystem : SharedInteractibleSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

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
           !TryComp<InteractibleComponent>(args.Target, out var targetComponent) )
            return;

        performerComponent.IsActive = false;
        targetComponent.IsActive = false;
        _actionBlocker.UpdateCanMove(args.User);
        _actionBlocker.UpdateCanMove(args.Target.Value);

        if (args.EndEvent != null)
        {
            args.EndEvent.Performer = args.User;
            args.EndEvent.Target = args.Target.Value;
            RaiseLocalEvent(args.EndEvent);
            RaiseNetworkEvent(args.EndEvent);
        }
    }

    private void OnInteraction(ExecutionInteractionEvent args)
    {
        if(!TryComp<InteractibleComponent>(args.Performer,out var performerComponent) || performerComponent.IsActive ||
           !TryComp<InteractibleComponent>(args.Target, out var targetComponent) || targetComponent.IsActive ||
           !_prototypeManager.TryIndex<InteractionActionPrototype>(args.EventName,out var eventPrototype))
            return;

        eventPrototype.ServerEvent.Performer = args.Performer;
        eventPrototype.ServerEvent.Target = args.Target;

        if (performerComponent.Action != null)
            _actions.RemoveAction(args.Performer, performerComponent.Action);

        var ev = eventPrototype.ServerEvent;
        RaiseLocalEvent(ev);

        if(eventPrototype.ServerEvent.Cancelled)
            return;

        RaiseNetworkEvent(ev);

        if (eventPrototype.InteractionTime > 0)
        {
            performerComponent.IsActive = true;
            targetComponent.IsActive = true;

            var doAfterArgs = new DoAfterArgs(args.Performer, TimeSpan.FromSeconds(eventPrototype.InteractionTime), new InteractionDoAfterEvent(eventPrototype.EndEvent),
                args.Performer, args.Target)
            {
                BreakOnHandChange = false,
            };

            if (eventPrototype.IsCloseInteraction)
            {
                _actionBlocker.UpdateCanMove(args.Performer);
                _actionBlocker.UpdateCanMove(args.Target);
                _transform.SetCoordinates(args.Performer,Transform(args.Target).Coordinates);
            }

            _doAfter.TryStartDoAfter(doAfterArgs);
        }

    }

    private void OnSelected(InteractionSelectMessage ev, EntitySessionEventArgs args)
    {
        var playerUid = args.SenderSession.AttachedEntity;
        if(!playerUid.HasValue || !TryComp<InteractibleComponent>(playerUid.Value,out var component)
                               || !component.AvailableInteractions.Contains(ev.SelectedInteraction)
           || !_prototypeManager.TryIndex<InteractionActionPrototype>(ev.SelectedInteraction, out var action)
                               || !TryComp<ActionsComponent>(playerUid,out var actionsComponent))
            return;

        component.Action = new EntityTargetAction(action)
        {
            Event = new ExecutionInteractionEvent(ev.SelectedInteraction)
        };


        _actions.AddAction(playerUid.Value,component.Action,null,actionsComponent);
        _actions.SetToggled(component.Action,true);
    }
}
