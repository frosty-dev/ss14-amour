using System.Numerics;
using Content.Shared.Movement.Components;
using Content.Shared.White.ShittyInteraction;
using Content.Shared.White.ShittyInteraction.Interactions;
using Robust.Client;
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
    protected override void OnEbat(EbatEvent ev)
    {
        base.OnEbat(ev);
        if(ev.Cancelled
           || !TryComp<InteractibleComponent>(ev.Performer,out var performerComponent)
           || !TryComp<InteractibleComponent>(ev.Target,out var targetComponent))
            return;

        Logger.Debug("IsClient! ");

        if (_animation.HasRunningAnimation(ev.Performer, InteractibleComponent.AnimationKey))
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
            Length = TimeSpan.FromMilliseconds(1750),
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

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                        new AnimationTrackProperty.KeyFrame(-rotation, 0.125f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.125f),

                    }
                }
            }
        };

        _animation.Play(ev.Performer, animation, InteractibleComponent.AnimationKey);

    }

    protected override void OnEndEbat(EbatEndEvent ev)
    {
        base.OnEndEbat(ev);
        Logger.Debug("IsClient tochno!");
    }

}
