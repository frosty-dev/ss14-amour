using Content.Server.Actions;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.White.Interaction;
using Content.Shared.White.Interaction.InteractionEvent;
using Robust.Shared.Timing;

namespace Content.Server.White.Interaction;

public sealed class InteractibleSystem : SharedInteractibleSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<InteractionSelectMessage>(OnSelected);
        SubscribeLocalEvent<InteractionPerformerComponent,BaseInteractionEvent>(OnInteraction);
        SubscribeLocalEvent<InteractionPerformerComponent,EbatEvent>(OnEbat);
    }

    private void OnInteraction(EntityUid uid, InteractionPerformerComponent component, BaseInteractionEvent args)
    {
        args.Event.Performer = args.Performer;
        args.Event.Target = args.Target;

        Logger.Debug(args.Performer + " " + args.Target + " Is meow " + args.Event.Target + " " + args.Event.Performer);

        RaiseLocalEvent(args.Performer,args.Event);
        RaiseLocalEvent(args.Target,args.Event);
    }

    private void OnEbat(EntityUid uid, InteractionPerformerComponent component, EbatEvent args)
    {
        Logger.Debug(args.Performer + " " + args.Target);
    }

    private void OnSelected(InteractionSelectMessage ev, EntitySessionEventArgs args)
    {
        var playerUid = args.SenderSession.AttachedEntity;
        if(!playerUid.HasValue || !TryComp<InteractionPerformerComponent>(playerUid.Value,out var component)
                               || !component.AvailableInteractions.Contains(ev.SelectedInteraction)
           || !_prototypeManager.TryIndex<InteractionActionPrototype>(ev.SelectedInteraction, out var action)
                               || !TryComp<ActionsComponent>(playerUid,out var actionsComponent))
            return;

        component.Action = new EntityTargetAction(action)
        {
            Event = new BaseInteractionEvent(action.Event!)
        };


        _actions.AddAction(playerUid.Value,component.Action,null,actionsComponent);
    }
}
