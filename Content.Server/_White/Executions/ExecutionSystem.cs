using Content.Server.DoAfter;
using Content.Server.Interaction;
using Content.Server.Kitchen.Components;
using Content.Server.Popups;
using Content.Shared._White.Executions;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._White.Executions;

public sealed class ExecutionSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly InteractionSystem _interactionSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private const float MeleeExecutionTimeModifier = 5.0f;
    private const float GunExecutionTime = 6.0f;
    private const float DamageModifier = 10.0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SharpComponent, GetVerbsEvent<UtilityVerb>>(OnGetInteractionVerbsMelee);
        SubscribeLocalEvent<GunComponent, GetVerbsEvent<UtilityVerb>>(OnGetInteractionVerbsGun);

        SubscribeLocalEvent<SharpComponent, ExecutionDoAfterEvent>(OnDoafterMelee);
        SubscribeLocalEvent<GunComponent, ExecutionDoAfterEvent>(OnDoafterGun);
    }

    private void OnGetInteractionVerbsMelee(
        EntityUid uid,
        SharpComponent component,
        GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        var attacker = args.User;
        var weapon = args.Using.Value;
        var victim = args.Target;

        if (!CanExecute(weapon, victim, attacker, true))
            return;

        EnsureComp<ExecutionComponent>(weapon, out var comp);

        if (IsDelayed(comp))
            return;

        UtilityVerb verb = new()
        {
            Act = () =>
            {
                TryStartMeleeExecutionDoafter(weapon, victim, attacker);
            },
            Impact = LogImpact.High,
            Text = Loc.GetString("execution-verb-name"),
            Message = Loc.GetString("execution-verb-message"),
        };

        args.Verbs.Add(verb);
    }

    private void OnGetInteractionVerbsGun(
        EntityUid uid,
        GunComponent component,
        GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        var attacker = args.User;
        var weapon = args.Using!.Value;
        var victim = args.Target;

        if (!CanExecute(weapon, victim, attacker, false))
            return;

        EnsureComp<ExecutionComponent>(weapon, out var comp);

        if (IsDelayed(comp))
            return;

        UtilityVerb verb = new()
        {
            Act = () =>
            {
                TryStartGunExecutionDoafter(weapon, victim, attacker);
            },
            Impact = LogImpact.High,
            Text = Loc.GetString("execution-verb-name"),
            Message = Loc.GetString("execution-verb-message"),
        };

        args.Verbs.Add(verb);
    }

    private bool CanExecuteWithAny(EntityUid victim, EntityUid attacker)
    {
        if (!HasComp<DamageableComponent>(victim))
            return false;

        if (!TryComp<MobStateComponent>(victim, out var mobState))
            return false;

        if (_mobStateSystem.IsDead(victim, mobState))
            return false;

        if (!_actionBlockerSystem.CanAttack(attacker, victim))
            return false;

        return victim == attacker || !_actionBlockerSystem.CanInteract(victim, null);
    }

    private bool CanExecute(EntityUid weapon, EntityUid victim, EntityUid user, bool isMelee)
    {
        if (!CanExecuteWithAny(victim, user))
            return false;

        if (isMelee)
        {
            return (TryComp<MeleeWeaponComponent>(weapon, out var melee) || melee!.Damage.GetTotal() <= 0.0f);
        }

        return (TryComp<GunComponent>(weapon, out var gun) || !_gunSystem.CanShoot(gun!));
    }


    private void TryStartMeleeExecutionDoafter(EntityUid weapon, EntityUid victim, EntityUid attacker)
    {
        if (!CanExecute(weapon, victim, attacker, true))
            return;

        var executionTime = (1.0f / Comp<MeleeWeaponComponent>(weapon).AttackRate) * MeleeExecutionTimeModifier;

        ShowExecutionPopup(
            attacker == victim ? "suicide-popup-melee-initial-external" : "execution-popup-melee-initial-external",
            PopupType.Medium,
            attacker,
            victim,
            weapon);

        var doAfter =
            new DoAfterArgs(EntityManager, attacker, executionTime, new ExecutionDoAfterEvent(), weapon, target: victim, used: weapon)
            {
                BreakOnMove = true,
                BreakOnHandChange = true,
                BreakOnDamage = true,
                NeedHand = true
            };

        _doAfterSystem.TryStartDoAfter(doAfter);

        EnsureComp<ExecutionComponent>(weapon, out var comp);

        comp.NextUse = _gameTiming.CurTime + comp.NextAttempt;
    }

    private void TryStartGunExecutionDoafter(EntityUid weapon, EntityUid victim, EntityUid attacker)
    {
        if (!CanExecute(weapon, victim, attacker, false))
            return;

        ShowExecutionPopup(
            attacker == victim ? "suicide-popup-gun-initial-external" : "execution-popup-gun-initial-external",
            PopupType.Medium,
            attacker,
            victim,
            weapon);

        var doAfter =
            new DoAfterArgs(EntityManager, attacker, GunExecutionTime, new ExecutionDoAfterEvent(), weapon, target: victim, used: weapon)
            {
                BreakOnMove = true,
                BreakOnHandChange = true,
                BreakOnDamage = true,
                NeedHand = true
            };

        _doAfterSystem.TryStartDoAfter(doAfter);

        EnsureComp<ExecutionComponent>(weapon, out var comp);

        comp.NextUse = _gameTiming.CurTime + comp.NextAttempt;
    }

    private void OnDoafterMelee(EntityUid uid, SharpComponent component, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used == null || args.Target == null)
            return;

        var attacker = args.User;
        var victim = args.Target.Value;
        var weapon = args.Used.Value;

        if (!CanExecute(weapon, victim, attacker, true))
            return;

        if (!TryComp<MeleeWeaponComponent>(weapon, out var melee) && melee!.Damage.GetTotal() > 0.0f)
            return;

        _damageableSystem.TryChangeDamage(victim, melee.Damage * DamageModifier, true);
        _audioSystem.PlayEntity(melee.HitSound, Filter.Pvs(weapon), weapon, true, AudioParams.Default);

        ShowExecutionPopup(
            attacker == victim ? "suicide-popup-melee-complete-external" : "execution-popup-melee-complete-external",
            PopupType.MediumCaution,
            attacker,
            victim,
            weapon);
    }

    private void OnDoafterGun(EntityUid uid, GunComponent component, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used == null || args.Target == null)
            return;

        var attacker = args.User;
        var weapon = args.Used.Value;
        var victim = args.Target.Value;

        if (!CanExecute(weapon, victim, attacker, false))
            return;

        var prevention = new ShotAttemptedEvent
        {
            User = attacker,
            Used = weapon!
        };

        RaiseLocalEvent(weapon, ref prevention);
        if (prevention.Cancelled)
            return;

        RaiseLocalEvent(attacker, ref prevention);
        if (prevention.Cancelled)
            return;

        var attemptEv = new AttemptShootEvent(attacker, null);
        RaiseLocalEvent(weapon, ref attemptEv);

        if (attemptEv is { Cancelled: true, Message: not null })
        {
            _popupSystem.PopupClient(attemptEv.Message, weapon, attacker);
            return;
        }

        var fromCoordinates = Transform(attacker).Coordinates;
        var ev = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), fromCoordinates, attacker);
        RaiseLocalEvent(weapon, ev);

        if (ev.Ammo.Count <= 0)
        {
            _audioSystem.PlayEntity(component.SoundEmpty, Filter.Pvs(weapon), weapon, true, AudioParams.Default);
            ShowExecutionPopup("execution-popup-gun-empty", PopupType.Medium, attacker, victim, weapon);
            return;
        }

        var damage = new DamageSpecifier();

        var ammoUid = ev.Ammo[0].Entity;
        switch (ev.Ammo[0].Shootable)
        {
            case CartridgeAmmoComponent cartridge:
                var prototype = _prototypeManager.Index<EntityPrototype>(cartridge.Prototype);
                prototype.TryGetComponent<ProjectileComponent>(out var projectileA, _componentFactory);
                if (projectileA != null)
                {
                    damage = projectileA.Damage * cartridge.Count;
                }

                cartridge.Spent = true;
                _appearanceSystem.SetData(ammoUid!.Value, AmmoVisuals.Spent, true);
                Dirty(ammoUid.Value, cartridge);

                break;

            case AmmoComponent:
                TryComp<ProjectileComponent>(ammoUid, out var projectileB);
                if (projectileB != null)
                {
                    damage = projectileB.Damage;
                }
                Del(ammoUid);
                break;

            case HitscanPrototype hitscan:
                damage = hitscan.Damage!;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (TryComp<ClumsyComponent>(attacker, out var clumsy) && !component.ClumsyProof)
        {
            if (_interactionSystem.TryRollClumsy(attacker, 0.3F, clumsy))
            {
                ShowExecutionPopup("execution-popup-gun-clumsy-external", PopupType.MediumCaution, attacker, victim, weapon);

                _damageableSystem.TryChangeDamage(attacker, damage, origin: attacker);
                _audioSystem.PlayEntity(component.SoundGunshot, Filter.Pvs(weapon), weapon, true, AudioParams.Default);
                return;
            }
        }

        _damageableSystem.TryChangeDamage(victim, damage * DamageModifier, true);
        _audioSystem.PlayEntity(component.SoundGunshot, Filter.Pvs(weapon), weapon, false, AudioParams.Default);

        ShowExecutionPopup(
            attacker != victim ? "execution-popup-gun-complete-external" : "suicide-popup-gun-complete-external",
            PopupType.LargeCaution,
            attacker,
            victim,
            weapon);

        args.Handled = true;
    }

    private void ShowExecutionPopup(string locString,
        PopupType type,
        EntityUid attacker,
        EntityUid victim,
        EntityUid weapon)
    {
        var message = Loc.GetString(locString,
            ("attacker", attacker),
            ("victim", victim),
            ("weapon", weapon));

        _popupSystem.PopupEntity(message, attacker, type);
    }

    private bool IsDelayed(ExecutionComponent comp)
    {
        return comp.NextUse > _gameTiming.CurTime;
    }
}
