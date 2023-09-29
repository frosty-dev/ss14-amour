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
        SubscribeLocalEvent<InteractibleComponent,EbatEvent>(OnEbat);
    }

    private void OnEbat(EntityUid uid, InteractibleComponent component, EbatEvent args)
    {
        Logger.Debug(args.Performer + " " + args.Target);
    }

    private void OnSelected(InteractionSelectMessage ev, EntitySessionEventArgs args)
    {
        var playerUid = args.SenderSession.AttachedEntity;
        if(!playerUid.HasValue || !TryComp<InteractibleComponent>(playerUid.Value,out var component) ||
           !component.AvailableInteractions.Contains(ev.SelectedInteraction) || !_prototypeManager.TryIndex<InteractionActionPrototype>(ev.SelectedInteraction,
               out var action) || !TryComp<ActionsComponent>(playerUid,out var actionsComponent))
            return;

        component.Action = new EntityTargetAction(action);

        _actions.AddAction(playerUid.Value,component.Action,null,actionsComponent);
    }
}
