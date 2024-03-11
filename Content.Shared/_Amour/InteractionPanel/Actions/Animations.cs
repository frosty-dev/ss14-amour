using System.Numerics;
using Content.Shared._Amour.Animation;
using Robust.Shared.Animations;

namespace Content.Shared._Amour.InteractionPanel.Actions;

[DataDefinition, Serializable]
public sealed partial class RequireAnimation : IInteractionAction
{
    [DataField] public float k0 = 0f;
    [DataField] public float k1 = 0f;
    [DataField] public float k2 = 0f;
    [DataField] public float k3 = 0f;
    [DataField] public float Length;
    [DataField] public int Repeat = 1;

    private void AnimateSomeShit(EntityUid uid, EntityUid target, IEntityManager entityManager)
    {
        var animationSystem = entityManager.System<SharedAnimationSystem>();

        var targetTransform = entityManager.GetComponent<TransformComponent>(target);
        var userTransform = entityManager.GetComponent<TransformComponent>(uid);

        var targetPos = targetTransform.LocalPosition;
        if (!targetTransform.ParentUid.Equals(targetTransform.GridUid))
            targetPos = entityManager.GetComponent<TransformComponent>(targetTransform.ParentUid).LocalPosition;


        var userPos = userTransform.LocalPosition;
        if (!userTransform.ParentUid.Equals(userTransform.GridUid))
            userPos = entityManager.GetComponent<TransformComponent>(userTransform.ParentUid).LocalPosition;

        var rotation = (targetPos - userPos)*0.5f;

        if (Length == 0)
        {
            Length = k0 + k1 + k2 + k3;
        }

        var animation = new Shared._Amour.Animation.Animation()
        {
            Length = TimeSpan.FromSeconds(Length*Repeat),
            AnimationTracks =
            {
                new AnimationTrackData()
                {
                    ComponentType = "Sprite",
                    Property = "Offset",
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                }
            }
        };

        for (var i = 0; i < Repeat; i++)
        {
            animation.AnimationTracks[0].KeyFrames.Add(animationSystem.KeyFrame(Vector2.Zero, k0));
            animation.AnimationTracks[0].KeyFrames.Add( animationSystem.KeyFrame(rotation, k1));
            animation.AnimationTracks[0].KeyFrames.Add(animationSystem.KeyFrame(rotation, k2));
            animation.AnimationTracks[0].KeyFrames.Add(animationSystem.KeyFrame(Vector2.Zero, k3));
        }

        animationSystem.Play(uid,animation);
    }

    public void Run(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        AnimateSomeShit(user,target,entityManager);
    }
}


[DataDefinition, Serializable]
public sealed partial class RequireHorizontalAnimation : IInteractionAction
{
    [DataField] public int Repeat = 6;
    [DataField] public Vector2 Shift = new (0, 0.5f);

    public void Run(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target, IEntityManager entityManager)
    {
        IoCManager.InjectDependencies(this);

        var animationSystem = entityManager.System<SharedAnimationSystem>();

        var rotation = (entityManager.GetComponent<TransformComponent>(target).LocalPosition - entityManager.GetComponent<TransformComponent>(user).LocalPosition)*0.5f;

        var animation = new Shared._Amour.Animation.Animation()
        {
            Length = TimeSpan.FromSeconds(0.5*Repeat + 0.25),
            AnimationTracks =
            {
                new AnimationTrackData()
                {
                    ComponentType = "Sprite",
                    Property = "Offset",
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        animationSystem.KeyFrame(Vector2.Zero,0)
                    }
                }
            }
        };

        for (var i = 0; i < Repeat; i++)
        {
            animation.AnimationTracks[0].KeyFrames.Add(animationSystem.KeyFrame(rotation-Shift*0.5f,0.25f));
            animation.AnimationTracks[0].KeyFrames.Add(animationSystem.KeyFrame(rotation-Shift,0.25f));
        }

        animation.AnimationTracks[0].KeyFrames.Add(animationSystem.KeyFrame(Vector2.Zero,0.25f));
        animationSystem.Play(user,animation);
    }
}

