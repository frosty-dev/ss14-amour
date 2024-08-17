using Content.Shared.Throwing;
using Robust.Shared.Random;

namespace Content.Shared._White.Explosion;

public sealed class SharedExplosionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ExplosionEffectComponent, ExplosiveTriggeredEvent>(OnExplosionEffectTriggered);
    }

    private void OnExplosionEffectTriggered(EntityUid uid, ExplosionEffectComponent component, ref ExplosiveTriggeredEvent args)
    {
        SpawnNextToOrDrop(component.ShockWave, uid);
        SpawnNextToOrDrop(component.Explosion, uid);

        if (component.MaxShrapnel <= 0)
            return;

        foreach (var effect in component.ShrapnelEffects)
        {
            var shrapnelCount = _random.Next(component.MinShrapnel, component.MaxShrapnel);
            for (var i = 0; i < shrapnelCount; i++)
            {
                var angle = _random.NextAngle();
                var direction = angle.ToVec().Normalized() * 10;
                var shrapnel = SpawnNextToOrDrop(effect, uid);
                _throwing.TryThrow(shrapnel, direction, component.ShrapnelSpeed / 10);
            }
        }
    }
}
