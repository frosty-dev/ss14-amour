using System.Linq;
using Content.Shared.Hands;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.White.Anus;
using Content.Shared.White.Genitals;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.White.ShittyInteraction;

public abstract class SharedInteractibleSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedAnusSystem Anus = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InteractibleComponent, ComponentInit>(OnInit);

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
        component.NextInteractionTime = Timing.CurTime;
    }

    public IEnumerable<string> GetAvailableInteractions(EntityUid uid,EntityUid target)
    {
        foreach (var proto in PrototypeManager.EnumeratePrototypes<InteractionActionPrototype>().ToList())
        {
            if(IsInteractionAvailable(uid,target,proto))
                yield return proto.ID;
        }
    }

    public bool IsInteractionAvailable(EntityUid uid, EntityUid target,InteractionAction proto)
    {
        if (proto.TargetRequirement.AssRequired && !Anus.HasAccessToButt(target)) return false;
        if (proto.PerformerRequirement.AssRequired && !Anus.HasAccessToButt(uid)) return false;

        if (proto.TargetRequirement.PenisRequired && !HasOgurec(target)) return false;
        if (proto.PerformerRequirement.PenisRequired && !HasOgurec(uid)) return false;

        if (proto.TargetRequirement.VaginaRequired && !HasPelmeshka(target)) return false;
        if (proto.PerformerRequirement.VaginaRequired && !HasPelmeshka(uid)) return false;

        return true;
    }

    public Gender GetGender(EntityUid uid, HumanoidAppearanceComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return Gender.Male;

        if(component.MarkingSet.Markings.TryGetValue(MarkingCategories.Genitals,out var genitals)
           && genitals.Count > 0 && PrototypeManager.TryIndex<GenitalPrototype>(genitals[0].ToString(),out var genitalPrototype))
            return genitalPrototype.Sex;

        return component.Gender;
    }

    public string GenderToString(Gender gender)
    {
        return gender.ToString().ToLowerInvariant();
    }

    public bool HasOgurec(EntityUid uid,HumanoidAppearanceComponent? component = null)
    {
        return GetGender(uid, component) == Gender.Male;
    }

    public bool HasPelmeshka(EntityUid uid,HumanoidAppearanceComponent? component = null)
    {
        return GetGender(uid, component) == Gender.Female;
    }
}
