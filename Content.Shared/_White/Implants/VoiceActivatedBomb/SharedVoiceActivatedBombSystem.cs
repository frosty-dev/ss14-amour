using Content.Shared.Body.Components;
using Content.Shared.Implants;
using Content.Shared.Tag;

namespace Content.Shared._White.Implants.VoiceActivatedBomb;

public abstract class SharedVoiceActivatedBombSystem : EntitySystem
{
    [Dependency] protected readonly TagSystem Tag = default!;
    protected const string VoiceActivatedBombTag = "VoiceActivatedBombImplant";
    public override void Initialize()
    {
        SubscribeLocalEvent<BodyComponent, AddImplantAttemptEvent>(OnTryInsertVoiceActivatedBomb);
    }

    private void OnTryInsertVoiceActivatedBomb(Entity<BodyComponent> ent, ref AddImplantAttemptEvent args)
    {
        if (!Tag.HasTag(args.Implant, VoiceActivatedBombTag))
            return;

        var ev = new InsertVoiceActivatedBombEvent(args.User, args.Implant, args.Implanter);
        RaiseLocalEvent(args.Implant, ev);
        if (ev.Cancelled)
            args.Cancel();

        return;
    }
}
public sealed class InsertVoiceActivatedBombEvent(EntityUid user, EntityUid implant, EntityUid implanter)
    : CancellableEntityEventArgs
{
    public readonly EntityUid User = user;
    public readonly EntityUid Implant = implant;
    public readonly EntityUid Implanter = implanter;
}
