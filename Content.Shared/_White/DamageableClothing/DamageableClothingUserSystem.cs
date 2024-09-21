using Content.Shared.Damage;

namespace Content.Shared.DamageableClothing;

/// <summary>
/// WD Engi Exclusive.
/// </summary>
public sealed partial class DamageableClothingSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private void InitializeUser()
    {
        SubscribeLocalEvent<DamageableClothingUserComponent, DamageModifyEvent>(OnUserDamageModified);
        SubscribeLocalEvent<DamageableClothingComponent, DamageModifyEvent>(OnDamageModified);
        SubscribeLocalEvent<DamageableClothingUserComponent, EntityTerminatingEvent>(OnEntityTerminating);
    }

    private void OnUserDamageModified(EntityUid uid, DamageableClothingUserComponent component, DamageModifyEvent args)
    {
        if (TryComp<DamageableClothingComponent>(component.ItemId, out var blocking))
        {
            if (args.Damage.GetTotal() <= 0)
                return;

            if (!TryComp<DamageableComponent>(component.ItemId, out var dmgComp))
                return;

            var blockFraction = 1;
            blockFraction = Math.Clamp(blockFraction, 0, 1);
            _damageable.TryChangeDamage(component.ItemId, blockFraction * args.OriginalDamage);
        }
    }

    private void OnDamageModified(EntityUid uid, DamageableClothingComponent component, DamageModifyEvent args)
    {
        var modifier = component.DamageModifier;
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifier);
    }

    private void OnEntityTerminating(EntityUid uid, DamageableClothingUserComponent component, ref EntityTerminatingEvent args)
    {
        if (!TryComp<DamageableClothingComponent>(component.ItemId, out var blockingComponent))
            return;

        RemCompDeferred<DamageableClothingUserComponent>(uid);
    }

}
