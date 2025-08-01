using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components; // WD
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared._White;
using Content.Shared._White.Implants.NeuroControl;
using Content.Shared.Movement.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using ItemToggleMeleeWeaponComponent = Content.Shared.Item.ItemToggle.Components.ItemToggleMeleeWeaponComponent;

namespace Content.Shared.Weapons.Melee;

public abstract class SharedMeleeWeaponSystem : EntitySystem
{
    [Dependency] protected readonly ISharedAdminLogManager   AdminLogger     = default!;
    [Dependency] protected readonly ActionBlockerSystem      Blocker         = default!;
    [Dependency] protected readonly SharedCombatModeSystem   CombatMode      = default!;
    [Dependency] protected readonly DamageableSystem         Damageable      = default!;
    [Dependency] protected readonly SharedInteractionSystem  Interaction     = default!;
    [Dependency] protected readonly IMapManager              MapManager      = default!;
    [Dependency] protected readonly SharedPopupSystem        PopupSystem     = default!;
    [Dependency] protected readonly IGameTiming              Timing          = default!;
    [Dependency] protected readonly SharedTransformSystem    TransformSystem = default!;
    [Dependency] private   readonly InventorySystem         _inventory       = default!;
    [Dependency] private   readonly MeleeSoundSystem        _meleeSound      = default!;
    [Dependency] private   readonly SharedPhysicsSystem     _physics         = default!;
    [Dependency] private   readonly IPrototypeManager       _protoManager    = default!;
    [Dependency] private   readonly StaminaSystem           _stamina         = default!;
    [Dependency] private   readonly IConfigurationManager   _cfg             = default!;

    private const int AttackMask = (int) (CollisionGroup.MobMask | CollisionGroup.Opaque);

    /// <summary>
    /// Maximum amount of targets allowed for a wide-attack.
    /// </summary>
    public const int MaxTargets = 5;

    private float DamageModifier { get; set; }

    private void SetDamage(float value) => DamageModifier = value;

    /// <summary>
    /// If an attack is released within this buffer it's assumed to be full damage.
    /// </summary>
    public const float GracePeriod = 0.05f;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(WhiteCVars.DamageModifier, SetDamage, true);

        SubscribeLocalEvent<MeleeWeaponComponent, HandSelectedEvent>(OnMeleeSelected);
        SubscribeLocalEvent<MeleeWeaponComponent, ShotAttemptedEvent>(OnMeleeShotAttempted);
        SubscribeLocalEvent<MeleeWeaponComponent, GunShotEvent>(OnMeleeShot);
        SubscribeLocalEvent<BonusMeleeDamageComponent, GetMeleeDamageEvent>(OnGetBonusMeleeDamage);
        SubscribeLocalEvent<BonusMeleeDamageComponent, GetHeavyDamageModifierEvent>(OnGetBonusHeavyDamageModifier);
        SubscribeLocalEvent<BonusMeleeAttackRateComponent, GetMeleeAttackRateEvent>(OnGetBonusMeleeAttackRate);

        SubscribeLocalEvent<ItemToggleMeleeWeaponComponent, ItemToggledEvent>(OnItemToggle);

        SubscribeAllEvent<HeavyAttackEvent>(OnHeavyAttack);
        SubscribeAllEvent<LightAttackEvent>(OnLightAttack);
        SubscribeAllEvent<DisarmAttackEvent>(OnDisarmAttack);
        SubscribeAllEvent<StopAttackEvent>(OnStopAttack);

#if DEBUG
        SubscribeLocalEvent<MeleeWeaponComponent,
                            MapInitEvent>                   (OnMapInit);
    }

    private void OnMapInit(EntityUid uid, MeleeWeaponComponent component, MapInitEvent args)
    {
        if (component.NextAttack > Timing.CurTime || component.NextMobAttack > Timing.CurTime) // WD EDIT
            Log.Warning($"Initializing a map that contains an entity that is on cooldown. Entity: {ToPrettyString(uid)}");
#endif
    }

    private void OnMeleeShotAttempted(EntityUid uid, MeleeWeaponComponent comp, ref ShotAttemptedEvent args)
    {
        if (comp.NextAttack > Timing.CurTime || comp.NextMobAttack > Timing.CurTime) // WD EDIT
            args.Cancel();
    }

    private void OnMeleeShot(EntityUid uid, MeleeWeaponComponent component, ref GunShotEvent args)
    {
        if (!TryComp<GunComponent>(uid, out var gun))
            return;

        // WD EDIT START
        var dirty = false;
        if (gun.NextFire > component.NextAttack)
        {
            component.NextAttack = gun.NextFire;
            dirty = true;
        }

        if (gun.NextFire > component.NextMobAttack)
        {
            component.NextMobAttack = gun.NextFire;
            dirty = true;
        }

        if (dirty)
            Dirty(uid, component);
        // WD EDIT END
    }

    private void OnMeleeSelected(EntityUid uid, MeleeWeaponComponent component, HandSelectedEvent args)
    {
        var attackRate = component.EquipCooldown ?? GetAttackRate(uid, args.User, component); // WD EDIT
        if (attackRate.Equals(0f))
            return;

        if (!component.ResetOnHandSelected)
            return;

        if (Paused(uid))
            return;

        // If someone swaps to this weapon then reset its cd.
        var curTime = Timing.CurTime;
        var minimum = curTime + TimeSpan.FromSeconds(1 / attackRate);

        // WD EDIT START
        var dirty = false;
        if (minimum > component.NextAttack)
        {
            component.NextAttack = minimum;
            dirty = true;
        }
        if (minimum > component.NextMobAttack)
        {
            component.NextMobAttack = minimum;
            dirty = true;
        }
        if (dirty)
            Dirty(uid, component);
        // WD EDIT END
    }

    private void OnGetBonusMeleeDamage(EntityUid uid, BonusMeleeDamageComponent component, ref GetMeleeDamageEvent args)
    {
        if (component.BonusDamage != null)
            args.Damage += component.BonusDamage;
        if (component.DamageModifierSet != null)
            args.Modifiers.Add(component.DamageModifierSet);
    }

    private void OnGetBonusHeavyDamageModifier(EntityUid uid, BonusMeleeDamageComponent component, ref GetHeavyDamageModifierEvent args)
    {
        args.DamageModifier += component.HeavyDamageFlatModifier;
        args.Multipliers *= component.HeavyDamageMultiplier;
    }

    private void OnGetBonusMeleeAttackRate(EntityUid uid, BonusMeleeAttackRateComponent component, ref GetMeleeAttackRateEvent args)
    {
        args.Rate += component.FlatModifier;
        args.Multipliers *= component.Multiplier;
    }

    private void OnStopAttack(StopAttackEvent msg, EntitySessionEventArgs args)
    {
        var user = args.SenderSession.AttachedEntity;

        if (user == null)
            return;

        if (!TryGetWeapon(user.Value, out var weaponUid, out var weapon) ||
            weaponUid != GetEntity(msg.Weapon))
        {
            return;
        }

        if (!weapon.Attacking)
            return;

        weapon.Attacking = false;
        Dirty(weaponUid, weapon);
    }

    private void OnLightAttack(LightAttackEvent msg, EntitySessionEventArgs args)
    {
        var user = args.SenderSession.AttachedEntity;

        if (user == null)
            return;

        if (!TryGetWeapon(user.Value, out var weaponUid, out var weapon) ||
            weaponUid != GetEntity(msg.Weapon))
        {
            return;
        }

        AttemptAttack(args.SenderSession.AttachedEntity!.Value, weaponUid, weapon, msg, args.SenderSession);
    }

    private void OnHeavyAttack(HeavyAttackEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity == null)
        {
            return;
        }

        if (!TryGetWeapon(args.SenderSession.AttachedEntity.Value, out var weaponUid, out var weapon) ||
            weaponUid != GetEntity(msg.Weapon))
        {
            return;
        }

        AttemptAttack(args.SenderSession.AttachedEntity.Value, weaponUid, weapon, msg, args.SenderSession);
    }

    private void OnDisarmAttack(DisarmAttackEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity == null)
        {
            return;
        }

        if (!TryGetWeapon(args.SenderSession.AttachedEntity.Value, out var weaponUid, out var weapon))
        {
            return;
        }

        AttemptAttack(args.SenderSession.AttachedEntity.Value, weaponUid, weapon, msg, args.SenderSession);
    }

    /// <summary>
    /// Gets the total damage a weapon does, including modifiers like wielding and enablind/disabling
    /// </summary>
    public DamageSpecifier GetDamage(EntityUid uid, EntityUid user, MeleeWeaponComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return new DamageSpecifier();

        var ev = new GetMeleeDamageEvent(uid, new (component.Damage), new(), user);
        RaiseLocalEvent(uid, ref ev);

        var modifiedDamage = ev.Damage * DamageModifier;

        return DamageSpecifier.ApplyModifierSets(modifiedDamage, ev.Modifiers);
    }

    public float GetAttackRate(EntityUid uid, EntityUid user, MeleeWeaponComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return 0;

        var ev = new GetMeleeAttackRateEvent(uid, component.AttackRate, 1, user);
        RaiseLocalEvent(uid, ref ev);

        return ev.Rate * ev.Multipliers;
    }

    public FixedPoint2 GetHeavyDamageModifier(EntityUid uid, EntityUid user, MeleeWeaponComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return FixedPoint2.Zero;

        var ev = new GetHeavyDamageModifierEvent(uid, component.ClickDamageModifier, 1, user);
        RaiseLocalEvent(uid, ref ev);

        return ev.DamageModifier * ev.Multipliers;
    }

    public bool TryGetWeapon(EntityUid entity, out EntityUid weaponUid, [NotNullWhen(true)] out MeleeWeaponComponent? melee)
    {
        weaponUid = default;
        melee = null;

        var ev = new GetMeleeWeaponEvent();
        RaiseLocalEvent(entity, ev);
        if (ev.Handled)
        {
            if (TryComp(ev.Weapon, out melee))
            {
                weaponUid = ev.Weapon.Value;
                return true;
            }

            return false;
        }

        // Use inhands entity if we got one.
        if (EntityManager.TryGetComponent(entity, out HandsComponent? hands) &&
            hands.ActiveHandEntity is { } held)
        {
            // Make sure the entity is a weapon AND it doesn't need
            // to be equipped to be used (E.g boxing gloves).
            if (EntityManager.TryGetComponent(held, out melee) &&
                !melee.MustBeEquippedToUse)
            {
                weaponUid = held;
                return true;
            }

            return false;
        }

        // Use hands clothing if applicable.
        if (_inventory.TryGetSlotEntity(entity, "gloves", out var gloves) &&
            TryComp<MeleeWeaponComponent>(gloves, out var glovesMelee))
        {
            weaponUid = gloves.Value;
            melee = glovesMelee;
            return true;
        }

        // Use our own melee
        if (TryComp(entity, out melee))
        {
            weaponUid = entity;
            return true;
        }

        return false;
    }

    public void AttemptLightAttackMiss(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, EntityCoordinates coordinates)
    {
        AttemptAttack(user, weaponUid, weapon, new LightAttackEvent(null, GetNetEntity(weaponUid), GetNetCoordinates(coordinates)), null);
    }

    public bool AttemptLightAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, EntityUid target)
    {
        if (!TryComp(target, out TransformComponent? targetXform))
            return false;

        return AttemptAttack(user, weaponUid, weapon, new LightAttackEvent(GetNetEntity(target), GetNetEntity(weaponUid), GetNetCoordinates(targetXform.Coordinates)), null);
    }

    public bool AttemptDisarmAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, EntityUid target)
    {
        if (!TryComp(target, out TransformComponent? targetXform))
            return false;

        return AttemptAttack(user, weaponUid, weapon, new DisarmAttackEvent(GetNetEntity(target), GetNetCoordinates(targetXform.Coordinates)), null);
    }

    private enum UpdateNextAttack : byte // WD
    {
        Mob,
        NonMob,
        Both
    }

    /// <summary>
    /// Called when a windup is finished and an attack is tried.
    /// </summary>
    /// <returns>True if attack successful</returns>
    private bool AttemptAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, AttackEvent attack, ICommonSession? session)
    {
        var curTime = Timing.CurTime;

        if (!CombatMode.IsInCombatMode(user))
            return false;

        var update = UpdateNextAttack.Both; // WD
        switch (attack)
        {
            case LightAttackEvent light:
                var lightTarget = GetEntity(light.Target);
                update = lightTarget == null ? UpdateNextAttack.Both :
                    IsMob(lightTarget.Value) ? UpdateNextAttack.Mob : UpdateNextAttack.NonMob; // WD

                if (!Blocker.CanAttack(user, lightTarget, (weaponUid, weapon)))
                    return false;

                // Can't self-attack if you're the weapon
                if (weaponUid == lightTarget)
                    return false;

                // WD START
                if (user == lightTarget && !weapon.CanAttackSelf)
                    return false;

                if (lightTarget == null)
                {
                    if (weapon.CanMiss || session == null) // NPCs can miss
                        break;
                    return false;
                }

                if (weapon.AttackWhitelist != null)
                {
                    if (!weapon.AttackWhitelist.IsValid(lightTarget.Value, EntityManager))
                        return false;
                }

                if (weapon.AttackBlacklist != null)
                {
                    if (weapon.AttackBlacklist.IsValid(lightTarget.Value, EntityManager))
                        return false;
                }
                // WD END

                break;
            case DisarmAttackEvent disarm:
                var disarmTarget = GetEntity(disarm.Target);
                update = disarmTarget == null ? UpdateNextAttack.Both :
                    IsMob(disarmTarget.Value) ? UpdateNextAttack.Mob : UpdateNextAttack.NonMob; // WD

                if (!Blocker.CanAttack(user, disarmTarget, (weaponUid, weapon), true))
                    return false;
                break;
            default:
                if (!Blocker.CanAttack(user, weapon: (weaponUid, weapon)))
                    return false;
                break;
        }

        // Windup time checked elsewhere.
        var fireRate = TimeSpan.FromSeconds(1f / GetAttackRate(weaponUid, user, weapon));

        var swings = 0;

        // WD EDIT START
        switch(update)
        {
            case UpdateNextAttack.Mob:
                if (weapon.NextMobAttack > curTime)
                    return false;
                break;
            case UpdateNextAttack.NonMob:
                if (weapon.NextAttack > curTime)
                    return false;
                break;
            default:
                if (weapon.NextAttack > curTime || weapon.NextMobAttack > curTime)
                    return false;
                break;
        }

        if (update != UpdateNextAttack.Mob)
        {
            // TODO: If we get autoattacks then probably need a shotcounter like guns so we can do timing properly.
            if (weapon.NextAttack < curTime)
                weapon.NextAttack = curTime;

            while (weapon.NextAttack <= curTime)
            {
                weapon.NextAttack += fireRate;
                swings++;
            }
        }

        if (update != UpdateNextAttack.NonMob)
        {
            if (weapon.NextMobAttack < curTime)
                weapon.NextMobAttack = curTime;

            while (weapon.NextMobAttack <= curTime)
            {
                weapon.NextMobAttack += fireRate;
                if (update == UpdateNextAttack.Mob)
                    swings++;
            }
        }
        // WD EDIT END

        Dirty(weaponUid, weapon);

        // Do this AFTER attack so it doesn't spam every tick
        var ev = new AttemptMeleeEvent {User = user}; // WD EDIT
        RaiseLocalEvent(weaponUid, ref ev);

        if (ev.Cancelled)
        {
            if (ev.Message != null)
            {
                PopupSystem.PopupClient(ev.Message, weaponUid, user);
            }

            return false;
        }

        // Attack confirmed
        for (var i = 0; i < swings; i++)
        {
            string animation;

            switch (attack)
            {
                case LightAttackEvent light:
                    DoLightAttack(user, light, weaponUid, weapon, session, out var miss); // WD EDIT
                    // WD EDIT START
                    animation = miss && weapon.Animation == "WeaponArcThrust"
                        ? weapon.MissAnimation
                        : weapon.Animation;
                    // WD EDIT END
                    break;
                case DisarmAttackEvent disarm:
                    if (!DoDisarm(user, disarm, weaponUid, weapon, session))
                        return false;

                    animation = weapon.DisarmAnimation; // WD EDIT
                    break;
                case HeavyAttackEvent heavy:
                    if (!DoHeavyAttack(user, heavy, weaponUid, weapon, session))
                        return false;

                    animation = weapon.WideAnimation;
                    break;
                default:
                    throw new NotImplementedException();
            }

            DoLungeAnimation(user, weaponUid, weapon.Angle, GetCoordinates(attack.Coordinates).ToMap(EntityManager, TransformSystem), weapon.Range, animation);
        }

        var attackEv = new MeleeAttackEvent(weaponUid);
        RaiseLocalEvent(user, ref attackEv);

        weapon.Attacking = true;
        return true;
    }

    protected abstract bool InRange(EntityUid user, EntityUid target, float range, ICommonSession? session);

    protected virtual void DoLightAttack(EntityUid user, LightAttackEvent ev, EntityUid meleeUid, MeleeWeaponComponent component, ICommonSession? session, out bool miss) // WD EDIT
    {
        miss = false; // WD EDIT
        // If I do not come back later to fix Light Attacks being Heavy Attacks you can throw me in the spider pit -Errant
        var damage = GetDamage(meleeUid, user, component) * GetHeavyDamageModifier(meleeUid, user, component);
        var target = GetEntity(ev.Target);

        // For consistency with wide attacks stuff needs damageable.
        if (Deleted(target) ||
            !HasComp<DamageableComponent>(target) ||
            !TryComp(target, out TransformComponent? targetXform) ||
            // Not in LOS.
            !InRange(user, target.Value, component.Range, session))
        {
            // Leave IsHit set to true, because the only time it's set to false
            // is when a melee weapon is examined. Misses are inferred from an
            // empty HitEntities.
            // TODO: This needs fixing
            if (meleeUid == user)
            {
                AdminLogger.Add(LogType.MeleeHit, LogImpact.Low,
                    $"{ToPrettyString(user):actor} melee attacked (light) using their hands and missed");
            }
            else
            {
                AdminLogger.Add(LogType.MeleeHit, LogImpact.Low,
                    $"{ToPrettyString(user):actor} melee attacked (light) using {ToPrettyString(meleeUid):tool} and missed");
            }
            var missEvent = new MeleeHitEvent(new List<EntityUid>(), user, meleeUid, damage, null);
            RaiseLocalEvent(meleeUid, missEvent);
            miss = true; // WD EDIT
            _meleeSound.PlaySwingSound(user, meleeUid, component);
            return;
        }

        // Sawmill.Debug($"Melee damage is {damage.Total} out of {component.Damage.Total}");

        // Raise event before doing damage so we can cancel damage if the event is handled
        var hitEvent = new MeleeHitEvent(new List<EntityUid> { target.Value }, user, meleeUid, damage, null);
        RaiseLocalEvent(meleeUid, hitEvent);

        if (hitEvent.Handled)
            return;

        var targets = new List<EntityUid>(1)
        {
            target.Value
        };

        var weapon = GetEntity(ev.Weapon);

        Interaction.DoContactInteraction(weapon, target);
        Interaction.DoContactInteraction(user, weapon);

        // If the user is using a long-range weapon, this probably shouldn't be happening? But I'll interpret melee as a
        // somewhat messy scuffle. See also, heavy attacks.
        Interaction.DoContactInteraction(user, target);

        // For stuff that cares about it being attacked.
        var attackedEvent = new AttackedEvent(meleeUid, user, targetXform.Coordinates);
        RaiseLocalEvent(target.Value, attackedEvent);

        var modifiedDamage = DamageSpecifier.ApplyModifierSets(damage + hitEvent.BonusDamage + attackedEvent.BonusDamage, hitEvent.ModifiersList);
        var damageResult = Damageable.TryChangeDamage(target, modifiedDamage, component.IgnoreResistances || hitEvent.PenetrateArmor, origin:user); // WD EDIT

        if (damageResult != null && damageResult.Any())
        {
            // If the target has stamina and is taking blunt damage, they should also take stamina damage based on their blunt to stamina factor
            if (damageResult.DamageDict.TryGetValue("Blunt", out var bluntDamage) && !HasComp<NeuroStabilizationComponent>(target.Value)) // WD EDIT
            {
                _stamina.TakeStaminaDamage(target.Value, (bluntDamage * component.BluntStaminaDamageFactor).Float(), visual: false, source: user, with: meleeUid == user ? null : meleeUid);
            }

            if (meleeUid == user)
            {
                AdminLogger.Add(LogType.MeleeHit, LogImpact.Medium,
                    $"{ToPrettyString(user):actor} melee attacked (light) {ToPrettyString(target.Value):subject} using their hands and dealt {damageResult.GetTotal():damage} damage");
            }
            else
            {
                AdminLogger.Add(LogType.MeleeHit, LogImpact.Medium,
                    $"{ToPrettyString(user):actor} melee attacked (light) {ToPrettyString(target.Value):subject} using {ToPrettyString(meleeUid):tool} and dealt {damageResult.GetTotal():damage} damage");
            }

        }

        _meleeSound.PlayHitSound(target.Value, user, GetHighestDamageSound(modifiedDamage, _protoManager), hitEvent.HitSoundOverride, component);

        if (damageResult?.GetTotal() > FixedPoint2.Zero)
        {
            DoDamageEffect(targets, user, targetXform);
        }
    }

    protected abstract void DoDamageEffect(List<EntityUid> targets, EntityUid? user,  TransformComponent targetXform);

    private bool DoHeavyAttack(EntityUid user, HeavyAttackEvent ev, EntityUid meleeUid, MeleeWeaponComponent component, ICommonSession? session)
    {
        // TODO: This is copy-paste as fuck with DoPreciseAttack
        if (!TryComp(user, out TransformComponent? userXform))
            return false;

        // WD EDIT
        if (TryComp<StaminaComponent>(user, out var stamina) && stamina.StaminaDamage >= stamina.CritThreshold)
            return false;
        // WD EDIT END

        var targetMap = GetCoordinates(ev.Coordinates).ToMap(EntityManager, TransformSystem);

        if (targetMap.MapId != userXform.MapID)
            return false;

        var userPos = TransformSystem.GetWorldPosition(userXform);
        var direction = targetMap.Position - userPos;
        var distance = Math.Min(component.Range, direction.Length());

        var damage = GetDamage(meleeUid, user, component) * 0.5f;
        var entities = GetEntityList(ev.Entities);

        // WD EDIT
        if (component.HeavyAttackStaminaCost > 0)
            _stamina.TakeStaminaDamage(user, component.HeavyAttackStaminaCost);
        // WD EDIT END

        if (entities.Count == 0)
        {
            if (meleeUid == user)
            {
                AdminLogger.Add(LogType.MeleeHit, LogImpact.Low,
                    $"{ToPrettyString(user):actor} melee attacked (heavy) using their hands and missed");
            }
            else
            {
                AdminLogger.Add(LogType.MeleeHit, LogImpact.Low,
                    $"{ToPrettyString(user):actor} melee attacked (heavy) using {ToPrettyString(meleeUid):tool} and missed");
            }
            var missEvent = new MeleeHitEvent(new List<EntityUid>(), user, meleeUid, damage, direction);
            RaiseLocalEvent(meleeUid, missEvent);

            // immediate audio feedback
            _meleeSound.PlaySwingSound(user, meleeUid, component);

            return true;
        }

        // Naughty input
        if (entities.Count > MaxTargets)
        {
            entities.RemoveRange(MaxTargets, entities.Count - MaxTargets);
        }

        // Validate client
        for (var i = entities.Count - 1; i >= 0; i--)
        {
            if (ArcRaySuccessful(entities[i], userPos, direction.ToWorldAngle(), component.Angle, distance,
                    userXform.MapID, user, session))
            {
                continue;
            }

            // Bad input
            entities.RemoveAt(i);
        }

        var targets = new List<EntityUid>();
        var damageQuery = GetEntityQuery<DamageableComponent>();

        foreach (var entity in entities)
        {
            if (entity == user ||
                !damageQuery.HasComponent(entity))
                continue;

            targets.Add(entity);
        }

        // Sawmill.Debug($"Melee damage is {damage.Total} out of {component.Damage.Total}");

        // Raise event before doing damage so we can cancel damage if the event is handled
        var hitEvent = new MeleeHitEvent(targets, user, meleeUid, damage, direction);
        RaiseLocalEvent(meleeUid, hitEvent);

        if (hitEvent.Handled)
            return true;

        var weapon = GetEntity(ev.Weapon);

        Interaction.DoContactInteraction(user, weapon);

        // For stuff that cares about it being attacked.
        foreach (var target in targets)
        {
            Interaction.DoContactInteraction(weapon, target);

            // If the user is using a long-range weapon, this probably shouldn't be happening? But I'll interpret melee as a
            // somewhat messy scuffle. See also, light attacks.
            Interaction.DoContactInteraction(user, target);
        }

        var appliedDamage = new DamageSpecifier();

        foreach (var entity in targets)
        {
            // We raise an attack attempt here as well,
            // primarily because this was an untargeted wideswing: if a subscriber to that event cared about
            // the potential target (such as for pacifism), they need to be made aware of the target here.
            // In that case, just continue.
            if (!Blocker.CanAttack(user, entity, (weapon, component)))
                continue;

            var attackedEvent = new AttackedEvent(meleeUid, user, GetCoordinates(ev.Coordinates));
            RaiseLocalEvent(entity, attackedEvent);
            var modifiedDamage = DamageSpecifier.ApplyModifierSets(damage + hitEvent.BonusDamage + attackedEvent.BonusDamage, hitEvent.ModifiersList);

            var damageResult = Damageable.TryChangeDamage(entity, modifiedDamage, component.IgnoreResistances || hitEvent.PenetrateArmor, origin:user); // WD EDIT

            if (damageResult != null && damageResult.GetTotal() > FixedPoint2.Zero)
            {
                appliedDamage += damageResult;

                if (meleeUid == user)
                {
                    AdminLogger.Add(LogType.MeleeHit, LogImpact.Medium,
                        $"{ToPrettyString(user):actor} melee attacked (heavy) {ToPrettyString(entity):subject} using their hands and dealt {damageResult.GetTotal():damage} damage");
                }
                else
                {
                    AdminLogger.Add(LogType.MeleeHit, LogImpact.Medium,
                        $"{ToPrettyString(user):actor} melee attacked (heavy) {ToPrettyString(entity):subject} using {ToPrettyString(meleeUid):tool} and dealt {damageResult.GetTotal():damage} damage");
                }
            }
        }

        if (entities.Count != 0)
        {
            var target = entities.First();
            _meleeSound.PlayHitSound(target, user, GetHighestDamageSound(appliedDamage, _protoManager), hitEvent.HitSoundOverride, component);
        }

        if (appliedDamage.GetTotal() > FixedPoint2.Zero)
        {
            DoDamageEffect(targets, user, Transform(targets[0]));
        }

        return true;
    }

    protected HashSet<EntityUid> ArcRayCast(Vector2 position, Angle angle, Angle arcWidth, float range, MapId mapId, EntityUid ignore)
    {
        // TODO: This is pretty sucky.
        var widthRad = arcWidth;
        var increments = 1 + 35 * (int) Math.Ceiling(widthRad / (2 * Math.PI));
        var increment = widthRad / increments;
        var baseAngle = angle - widthRad / 2;

        var resSet = new HashSet<EntityUid>();

        for (var i = 0; i < increments; i++)
        {
            var castAngle = new Angle(baseAngle + increment * i);
            var res = _physics.IntersectRay(mapId,
                new CollisionRay(position, castAngle.ToWorldVec(),
                    AttackMask), range, ignore, false).ToList();

            if (res.Count != 0)
            {
                resSet.Add(res[0].HitEntity);
            }
        }

        return resSet;
    }

    protected virtual bool ArcRaySuccessful(EntityUid targetUid, Vector2 position, Angle angle, Angle arcWidth, float range,
        MapId mapId, EntityUid ignore, ICommonSession? session)
    {
        // Only matters for server.
        return true;
    }


    public static string? GetHighestDamageSound(DamageSpecifier modifiedDamage, IPrototypeManager protoManager)
    {
        var groups = modifiedDamage.GetDamagePerGroup(protoManager);

        // Use group if it's exclusive, otherwise fall back to type.
        if (groups.Count == 1)
        {
            return groups.Keys.First();
        }

        var highestDamage = FixedPoint2.Zero;
        string? highestDamageType = null;

        foreach (var (type, damage) in modifiedDamage.DamageDict)
        {
            if (damage <= highestDamage)
                continue;

            highestDamageType = type;
        }

        return highestDamageType;
    }

    protected virtual bool DoDisarm(EntityUid user, DisarmAttackEvent ev, EntityUid meleeUid, MeleeWeaponComponent component, ICommonSession? session)
    {
        var target = GetEntity(ev.Target);

        if (Deleted(target) ||
            user == target)
        {
            return false;
        }

        // Play a sound to give instant feedback; same with playing the animations
        _meleeSound.PlaySwingSound(user, meleeUid, component);
        return true;
    }

    private void DoLungeAnimation(EntityUid user, EntityUid weapon, Angle angle, MapCoordinates coordinates, float length, string? animation)
    {
        // TODO: Assert that offset eyes are still okay.
        if (!TryComp(user, out TransformComponent? userXform))
            return;

        var invMatrix = TransformSystem.GetInvWorldMatrix(userXform);
        var localPos = invMatrix.Transform(coordinates.Position);

        if (localPos.LengthSquared() <= 0f)
            return;

        localPos = userXform.LocalRotation.RotateVec(localPos);

        // We'll play the effect just short visually so it doesn't look like we should be hitting but actually aren't.
        const float bufferLength = 0.2f;
        var visualLength = length - bufferLength;

        if (localPos.Length() > visualLength)
            localPos = localPos.Normalized() * visualLength;

        DoLunge(user, weapon, angle, localPos, animation);
    }

    public abstract void DoLunge(EntityUid user, EntityUid weapon, Angle angle, Vector2 localPos, string? animation, bool predicted = true);

    /// <summary>
    /// Used to update the MeleeWeapon component on item toggle.
    /// </summary>
    private void OnItemToggle(EntityUid uid, ItemToggleMeleeWeaponComponent itemToggleMelee, ItemToggledEvent args)
    {
        if (!TryComp(uid, out MeleeWeaponComponent? meleeWeapon))
            return;

        if (args.Activated)
        {
            if (itemToggleMelee.ActivatedDamage != null)
            {
                //Setting deactivated damage to the weapon's regular value before changing it.
                itemToggleMelee.DeactivatedDamage ??= meleeWeapon.Damage;
                meleeWeapon.Damage = itemToggleMelee.ActivatedDamage;
            }

            meleeWeapon.HitSound = itemToggleMelee.ActivatedSoundOnHit;

            if (itemToggleMelee.ActivatedSoundOnHitNoDamage != null)
            {
                //Setting the deactivated sound on no damage hit to the weapon's regular value before changing it.
                itemToggleMelee.DeactivatedSoundOnHitNoDamage ??= meleeWeapon.NoDamageSound;
                meleeWeapon.NoDamageSound = itemToggleMelee.ActivatedSoundOnHitNoDamage;
            }

            if (itemToggleMelee.ActivatedSoundOnSwing != null)
            {
                //Setting the deactivated sound on no damage hit to the weapon's regular value before changing it.
                itemToggleMelee.DeactivatedSoundOnSwing ??= meleeWeapon.SwingSound;
                meleeWeapon.SwingSound = itemToggleMelee.ActivatedSoundOnSwing;
            }

            if (itemToggleMelee.DeactivatedSecret)
                meleeWeapon.Hidden = false;
        }
        else
        {
            if (itemToggleMelee.DeactivatedDamage != null)
                meleeWeapon.Damage = itemToggleMelee.DeactivatedDamage;

            meleeWeapon.HitSound = itemToggleMelee.DeactivatedSoundOnHit;

            if (itemToggleMelee.DeactivatedSoundOnHitNoDamage != null)
                meleeWeapon.NoDamageSound = itemToggleMelee.DeactivatedSoundOnHitNoDamage;

            if (itemToggleMelee.DeactivatedSoundOnSwing != null)
                meleeWeapon.SwingSound = itemToggleMelee.DeactivatedSoundOnSwing;

            if (itemToggleMelee.DeactivatedSecret)
                meleeWeapon.Hidden = true;
        }

        Dirty(uid, meleeWeapon);
    }

    protected bool IsMob(EntityUid uid)
    {
        return HasComp<InputMoverComponent>(uid) || HasComp<MobMoverComponent>(uid) || HasComp<CombatModeComponent>(uid);
    }
}
