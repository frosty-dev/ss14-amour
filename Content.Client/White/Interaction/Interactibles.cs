using System.Numerics;
using Content.Shared.Movement.Components;
using Content.Shared.White.ShittyInteraction;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Animations;
using static Content.Shared.White.ShittyInteraction.InteractibleComponent;

namespace Content.Client.White.Interaction;

public sealed class Interactibles : SharedInteractibles
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    protected override void OnCome(ComeToTargetEvent ev)
    {
        base.OnCome(ev);
        if(ev.Cancelled
           || !TryComp<InteractibleComponent>(ev.Performer,out var performerComponent)
           || !TryComp<InteractibleComponent>(ev.Target,out var targetComponent))
            return;

        if (_animation.HasRunningAnimation(ev.Performer, AnimationKey))
            return;

        var viewer = _playerManager.LocalPlayer?.ControlledEntity;
        if(!viewer.HasValue)
            return;

        if(!TryComp<InputMoverComponent>(viewer,out var moverComponent))
            return;

        var viewerRot = moverComponent.TargetRelativeRotation;
        var rotation = (Transform(ev.Performer).LocalRotation - viewerRot).ToWorldVec()*0.25f;

        var animation = new Animation
        {
            Length = TimeSpan.FromSeconds(3),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0),
                        new AnimationTrackProperty.KeyFrame(rotation, 0.150f),
                        new AnimationTrackProperty.KeyFrame(rotation, 1.250f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.150f),
                    }
                }
            }
        };

        _animation.Play(ev.Performer, animation, AnimationKey);
    }

    protected override void OnShlifovka(ShlifovkaEvent ev)
    {
        base.OnShlifovka(ev);
        if(ev.Cancelled
           || !TryComp<InteractibleComponent>(ev.Performer,out var performerComponent)
           || !TryComp<InteractibleComponent>(ev.Target,out var targetComponent))
            return;

        if (_animation.HasRunningAnimation(ev.Performer, AnimationKey))
                return;

        var viewer = _playerManager.LocalPlayer?.ControlledEntity;
        if(!viewer.HasValue)
            return;

        if(!TryComp<InputMoverComponent>(viewer,out var moverComponent))
            return;

        var viewerRot = moverComponent.TargetRelativeRotation;
        var rotation1 = -(Transform(ev.Performer).LocalRotation - viewerRot).ToWorldVec()*0.1f;

        var rotation = new Vector2(0,0.5f);

        var animation = new Animation
        {
            Length = TimeSpan.FromSeconds(5),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.250f),
                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.250f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation, 0.250f),
                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.250f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.125f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.125f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.125f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.125f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation, 0.500f),
                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.500f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation, 0.500f),
                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.500f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation, 0.250f),
                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.250f),

                        new AnimationTrackProperty.KeyFrame(rotation1-rotation*0.5f, 0.250f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.250f),

                    }
                }
            }
        };

        _animation.Play(ev.Performer, animation, InteractibleComponent.AnimationKey);
    }

    protected override void OnShlep(ShlepButtEvent ev)
    {
        base.OnShlep(ev);
        if(ev.Cancelled
           || !TryComp<InteractibleComponent>(ev.Performer,out var performerComponent)
           || !TryComp<InteractibleComponent>(ev.Target,out var targetComponent))
            return;

        if (_animation.HasRunningAnimation(ev.Performer, AnimationKey))
                return;

        var viewer = _playerManager.LocalPlayer?.ControlledEntity;
        if(!viewer.HasValue)
            return;

        if(!TryComp<InputMoverComponent>(viewer,out var moverComponent))
            return;

        var viewerRot = moverComponent.TargetRelativeRotation;
        var rotation = (Transform(ev.Performer).LocalRotation - viewerRot).ToWorldVec()*0.25f;

        var animation = new Animation
        {
            Length = TimeSpan.FromMilliseconds(500),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),

                        new AnimationTrackProperty.KeyFrame(rotation, 0.150f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.250f),
                    }
                }
            }
        };

        _animation.Play(ev.Performer, animation, AnimationKey);

    }

    protected override void OnEbat(EbatEvent ev)
    {
        base.OnEbat(ev);
        if(ev.Cancelled
           || !TryComp<InteractibleComponent>(ev.Performer,out var performerComponent)
           || !TryComp<InteractibleComponent>(ev.Target,out var targetComponent))
            return;

        if (_animation.HasRunningAnimation(ev.Performer, AnimationKey))
                return;

        var viewer = _playerManager.LocalPlayer?.ControlledEntity;
        if(!viewer.HasValue)
            return;

        if(!TryComp<InputMoverComponent>(viewer,out var moverComponent))
            return;

        var viewerRot = moverComponent.TargetRelativeRotation;
        var rotation = (Transform(ev.Performer).LocalRotation - viewerRot).ToWorldVec()*0.25f;

        var animation = new Animation
        {
            Length = TimeSpan.FromSeconds(5),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.250f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.250f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.250f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.250f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.500f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.500f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.500f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.500f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.250f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.250f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.250f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.250f),

                    }
                }
            }
        };

        _animation.Play(ev.Performer, animation, InteractibleComponent.AnimationKey);

    }
}
