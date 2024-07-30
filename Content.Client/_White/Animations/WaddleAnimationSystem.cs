using System.Numerics;
using Content.Shared._White.Animations;
using Content.Shared.Movement.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client._White.Animations;

public sealed class WaddleAnimationSystem : SharedWaddledAnimationSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<WaddleAnimationComponent, AnimationCompletedEvent>(OnAnimationCompleted);

        SubscribeAllEvent<StartedWaddlingEvent>(ev => PlayAnimation(GetEntity(ev.User)));
        SubscribeAllEvent<StoppedWaddlingEvent>(ev => StopAnimation(GetEntity(ev.User)));
    }

    protected override void PlayAnimation(EntityUid uid)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        if (!TryComp<WaddleAnimationComponent>(uid, out var component))
            return;

        if (!TryComp<InputMoverComponent>(uid, out var mover))
            return;

        if (_animation.HasRunningAnimation(uid, component.KeyName))
            return;

        var tumbleIntensity = component.LastStep ? 360 - component.TumbleIntensity : component.TumbleIntensity;
        var len = mover.Sprinting ? component.AnimationLength * component.RunAnimationLengthMultiplier : component.AnimationLength;

        component.LastStep = !component.LastStep;
        component.IsCurrentlyWaddling = true;

        var anim = new Animation()
        {
            Length = TimeSpan.FromSeconds(len),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), 0),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(tumbleIntensity), len/3),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), len/3),
                    }
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(), 0),
                        new AnimationTrackProperty.KeyFrame(component.HopIntensity, len/3),
                        new AnimationTrackProperty.KeyFrame(new Vector2(), len/3),
                    }
                }
            }
        };

        _animation.Play(uid, anim, component.KeyName);
    }

    protected override void StopAnimation(EntityUid uid)
    {
        if (!TryComp<WaddleAnimationComponent>(uid, out var component))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _animation.Stop(uid, component.KeyName);

        sprite.Offset = new Vector2();
        sprite.Rotation = Angle.FromDegrees(0);
        component.IsCurrentlyWaddling = false;
    }

    private void OnAnimationCompleted(EntityUid uid, WaddleAnimationComponent component, AnimationCompletedEvent args)
    {
        if (args.Key != component.KeyName)
            return;

        PlayAnimation(uid);
    }
}
