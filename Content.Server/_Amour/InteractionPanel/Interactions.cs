using System.Numerics;
using Content.Server._Amour.Animation;
using Content.Shared._Amour.Animation;
using Content.Shared._Amour.InteractionPanel;
using Content.Shared.Movement.Components;
using Robust.Shared.Animations;

namespace Content.Server._Amour.InteractionPanel;

public sealed class Interactions : EntitySystem
{
    [Dependency] private readonly SharebleAnimationSystem _animationSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<InteractionPanelComponent,InteractionBeginningEvent>(OnBegin);
        SubscribeLocalEvent<InteractionPanelComponent,InteractionEndingEvent>(OnEnd);
    }

    private void OnEnd(EntityUid uid, InteractionPanelComponent component, InteractionEndingEvent args)
    {
        if(args.Handled)
            return;

        switch (args.Id)
        {

        }
    }

    private void OnBegin(EntityUid uid, InteractionPanelComponent component, InteractionBeginningEvent args)
    {
        if(args.Handled)
            return;

        switch (args.Id)
        {
            case "SlapButt":
                OnSlapButt(uid,component,args);
                break;
        }
    }

    private void OnSlapButt(EntityUid uid, InteractionPanelComponent component, InteractionBeginningEvent args)
    {
        var rotation = (Transform(args.Target).LocalPosition - Transform(args.Performer).LocalPosition)*0.5f;

        var animation = new Shared._Amour.Animation.Animation()
        {
            Length = TimeSpan.FromSeconds(0.5),
            AnimationTracks =
            {
                new AnimationTrackData()
                {
                    ComponentType = "Sprite",
                    Property = "Offset",
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        _animationSystem.KeyFrame(Vector2.Zero, 0),
                        _animationSystem.KeyFrame(rotation, 0.100f),
                        _animationSystem.KeyFrame(Vector2.Zero, 0.250f)
                    }
                }
            }
        };

        _animationSystem.Play(uid,animation);
    }
}
