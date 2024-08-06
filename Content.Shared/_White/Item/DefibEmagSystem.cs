using Content.Shared._White.Item.DelayedKnockdown;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Medical;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;

namespace Content.Shared._White.Item;

public sealed class DefibEmagSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DefibrillatorComponent, GotEmaggedEvent>(OnEmag);
    }

    private void OnEmag(Entity<DefibrillatorComponent> ent, ref GotEmaggedEvent args)
    {
        var (uid, comp) = ent;
        comp.ZapDamage = 55;
        comp.ZapDelay = TimeSpan.FromSeconds(1);
        comp.WritheDuration = TimeSpan.FromSeconds(6);
        var meleeWeapon = new MeleeWeaponComponent
        {
            Hidden = true,
            AttackRate = 0.2f,
            CanHeavyAttack = false,
            AttackWhitelist = comp.EmaggedAttackWhitelist,
            EquipCooldown = 1f,
            Damage = new DamageSpecifier()
        };
        var delayedKnockdown = new DelayedKnockdownOnHitComponent
        {
            Delay = TimeSpan.Zero,
            KnockdownTime = TimeSpan.FromSeconds(1.5)
        };
        var staminaDamage = new StaminaDamageOnHitComponent
        {
            Damage = 60f,
            Sound = new SoundCollectionSpecifier("sparks")
        };
        AddComp(uid, meleeWeapon, true);
        AddComp(uid, delayedKnockdown, true);
        AddComp(uid, staminaDamage, true);
        args.Handled = true;
    }
}
