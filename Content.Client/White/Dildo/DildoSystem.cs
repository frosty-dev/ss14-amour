using System.Numerics;
using Content.Shared.White.Dildo;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client.White.Dildo;

public sealed class DildoSystem : SharedDildoSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationSystem = default!;
    private readonly string _vibration = "vibration";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DildoComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnAnimationCompleted(EntityUid uid, DildoComponent component, AnimationCompletedEvent args)
    {
        if(args.Key != _vibration || !component.IsVibrating)
            return;

        _animationSystem.Play(uid,GetAnimation(), _vibration);
    }

    public override void ToggleVibrate(EntityUid uid, DildoComponent component)
    {
        if (component.IsVibrating)
            _animationSystem.Play(uid,GetAnimation(), _vibration);

    }

    private Animation GetAnimation()
    {
        return new Animation
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
