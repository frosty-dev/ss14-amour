using System.Numerics;
using Content.Shared._Amour.Vibrator;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client._Amour.Vibrator;

public sealed class VibratorSystem : SharedVibratorSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationSystem = default!;
    private readonly string _vibration = "vibration";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VibratorComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnAnimationCompleted(EntityUid uid, VibratorComponent component, AnimationCompletedEvent args)
    {
        if (args.Key != _vibration || !component.IsVibrating)
            return;

        if (_animationSystem.HasRunningAnimation(uid, _vibration))
            return;

        _animationSystem.Play(uid, GetAnimation(), _vibration);
    }

    public override void ToggleVibrate(EntityUid uid, VibratorComponent component)
    {
        if (_animationSystem.HasRunningAnimation(uid, _vibration))
            return;

        if (component.IsVibrating)
            _animationSystem.Play(uid, GetAnimation(), _vibration);

    }

    private Robust.Client.Animations.Animation GetAnimation()
    {
        return new Robust.Client.Animations.Animation
        {
            Length = TimeSpan.FromMilliseconds(100),
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

                        new AnimationTrackProperty.KeyFrame(new Vector2(0.1f, 0), 0.25f),

                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.25f),

                        new AnimationTrackProperty.KeyFrame(new Vector2(-0.1f, 0), 0.25f),

                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.25f),
                    }
                }
            }
        };
    }
}
