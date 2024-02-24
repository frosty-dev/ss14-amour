using Content.Shared._Amour.Animation;
using Content.Shared._Amour.Hole;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Server._Amour.Animation;

public sealed class SharebleAnimationSystem : SharedAnimationSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HoleContainerComponent,GetVerbsEvent<Verb>>(OnVerb);
    }

    private void OnVerb(EntityUid uid, HoleContainerComponent component, GetVerbsEvent<Verb> args)
    {
        if(!_prototypeManager.TryIndex<AnimationPrototype>("meow",out var prototype))
            return;

        args.Verbs.Add(new Verb()
        {
            Text = "MEOW ABOUT IT",
            Act = () => Play(uid, prototype)
        });
    }

    public override void Play(EntityUid uid, ProtoId<AnimationPrototype> protoId)
    {
        RaiseNetworkEvent(new AnimationProtoStartMessage(GetNetEntity(uid),protoId));
    }

    public override void Play(EntityUid uid, AnimationData data, string animationId = "funny")
    {
        RaiseNetworkEvent(new AnimationStartMessage(GetNetEntity(uid),data,animationId));
    }
}
