using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared.White.Interaction;

public abstract class SharedInteractibleSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager _prototypeManager = default!;

    public List<string> AvailableInteractions = new List<string>();
    public override void Initialize()
    {
        foreach (var interaction in _prototypeManager.EnumeratePrototypes<InteractionActionPrototype>())
        {
            AvailableInteractions.Add(interaction.ID);
        }

        SubscribeLocalEvent<InteractibleComponent,ComponentInit>(OnInit);
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
