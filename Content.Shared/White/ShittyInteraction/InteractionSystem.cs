using System.Linq;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared.White.ShittyInteraction;

public abstract class SharedInteractibleSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;

    public List<string> AvailableInteractions = new List<string>();
    public override void Initialize()
    {
        foreach (var interaction in PrototypeManager.EnumeratePrototypes<InteractionActionPrototype>())
        {
            AvailableInteractions.Add(interaction.ID);
        }

        SubscribeLocalEvent<InteractibleComponent,ComponentInit>(OnInit);

        SubscribeLocalEvent<InteractibleComponent, UseAttemptEvent>(OnCancel);
        SubscribeLocalEvent<InteractibleComponent, DropAttemptEvent>(OnCancel);
        SubscribeLocalEvent<InteractibleComponent, PickupAttemptEvent>(OnCancel);
        SubscribeLocalEvent<InteractibleComponent, UpdateCanMoveEvent>(OnCancel);
        SubscribeLocalEvent<InteractibleComponent, ChangeDirectionAttemptEvent>(OnCancel);
    }

    private void OnCancel(EntityUid uid, InteractibleComponent component, CancellableEntityEventArgs args)
    {
        if(component.IsActive)
            args.Cancel();
    }

    private void OnInit(EntityUid uid, InteractibleComponent component, ComponentInit args)
    {
        component.AvailableInteractions = AvailableInteractions.ToList();
    }

    public List<string> TryGetAvailableInteractions(EntityUid uid, InteractibleComponent? component = null)
    {
        return !Resolve(uid, ref component) ? new() : component.AvailableInteractions;
    }
}
