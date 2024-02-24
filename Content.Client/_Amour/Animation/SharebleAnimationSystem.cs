using System.Linq;
using Content.Client.Light.Components;
using Content.Shared._Amour.Animation;
using Content.Shared._Amour.Hole;
using Content.Shared.Verbs;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;

namespace Content.Client._Amour.Animation;

public sealed class SharebleAnimationSystem : SharedAnimationSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<AnimationStartMessage>(OnStart);
        SubscribeNetworkEvent<AnimationProtoStartMessage>(OnProtoStart);
    }

    private void OnProtoStart(AnimationProtoStartMessage ev)
    {
        Play(GetEntity(ev.Owner),ev.ProtoId);
    }

    private void OnStart(AnimationStartMessage ev)
    {
        Play(GetEntity(ev.Owner),ev.Data,ev.Id);
    }

    public override void Play(EntityUid uid,ProtoId<AnimationPrototype> protoId)
    {
        if(!_prototypeManager.TryIndex(protoId, out var prototype))
            return;

        Play(uid, prototype, protoId);
    }

    public override void Play(EntityUid uid,AnimationData data, string animationId = "funny")
    {
        if(_animation.HasRunningAnimation(uid,animationId))
            return;

        var animation = ParseAnimation(data);
        _animation.Play(uid,animation,animationId);
    }

    public Robust.Client.Animations.Animation ParseAnimation(AnimationData data)
    {
        var animation = new Robust.Client.Animations.Animation
        {
            Length = data.Length
        };

        foreach (var track in data.AnimationTracks)
        {
            var component = _componentFactory.GetComponent(track.ComponentType);
            var componentType = component.GetType();
            var propertyType = AnimationHelper.GetAnimatableProperty(component, track.Property)?.GetType();

            if (propertyType is null)
            {
                Logger.Error($"OH FUCK SEMPAI~~ PROPERTY FOR ANIMATION NOT FOUND: {track.Property} in component: {track.ComponentType}");
                continue;
            }

            var property = new AnimationTrackComponentProperty()
            {
                ComponentType = componentType,
                InterpolationMode = track.InterpolationMode,
                Property = track.Property
            };


            foreach (var key in track.KeyFrames)
            {
                var value = _serializationManager.Read(propertyType, new ValueDataNode(key.Value));
                if (value is null)
                {
                    Logger.Error($"FUCK HARDER, SEMPAI~~ value not found: {key.Value}");
                    continue;
                }

                property.KeyFrames.Add(new AnimationTrackProperty.KeyFrame(value, key.KeyTime));
            }
            animation.AnimationTracks.Add(property);
        }

        return animation;
    }
}
