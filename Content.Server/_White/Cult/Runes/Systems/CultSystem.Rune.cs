using System.Linq;
using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Chat.Systems;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Hands.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Server._White.Cult.GameRule;
using Content.Server._White.Cult.Runes.Comps;
using Content.Server._White.Cult.UI;
using Content.Server.Antag;
using Content.Server.Bible.Components;
using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Fluids.Components;
using Content.Server.Ghost;
using Content.Server.Pinpointer;
using Content.Server.Revenant.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared._White.Cult;
using Content.Shared._White.Cult.Components;
using Content.Shared._White.Cult.Runes;
using Content.Shared._White.Cult.UI;
using Content.Shared.Cuffs;
using Content.Shared.GameTicking;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using CultistComponent = Content.Shared._White.Cult.Components.CultistComponent;

namespace Content.Server._White.Cult.Runes.Systems;

public sealed partial class CultSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly CultRuleSystem _ruleSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly FlammableSystem _flammableSystem = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _thresholdSystem = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Runes
        SubscribeLocalEvent<CultRuneBaseComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<CultRuneOfferingComponent, CultRuneInvokeEvent>(OnInvokeOffering);
        SubscribeLocalEvent<CultRuneBuffComponent, CultRuneInvokeEvent>(OnInvokeBuff);
        SubscribeLocalEvent<CultRuneTeleportComponent, CultRuneInvokeEvent>(OnInvokeTeleport);
        SubscribeLocalEvent<CultRuneApocalypseComponent, CultRuneInvokeEvent>(OnInvokeApocalypse);
        SubscribeLocalEvent<CultRuneReviveComponent, CultRuneInvokeEvent>(OnInvokeRevive);
        SubscribeLocalEvent<CultRuneBarrierComponent, CultRuneInvokeEvent>(OnInvokeBarrier);
        SubscribeLocalEvent<CultRuneSummoningComponent, CultRuneInvokeEvent>(OnInvokeSummoning);
        SubscribeLocalEvent<CultRuneBloodBoilComponent, CultRuneInvokeEvent>(OnInvokeBloodBoil);
        SubscribeLocalEvent<CultistComponent, SummonNarsieDoAfterEvent>(NarsieSpawn);

        SubscribeLocalEvent<CultEmpowerComponent, CultEmpowerSelectedBuiMessage>(OnEmpowerSelected);
        SubscribeLocalEvent<CultEmpowerComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<CultEmpowerComponent, CultRuneInvokeEvent>(OnActiveInWorld);

        // UI
        SubscribeLocalEvent<RuneDrawerProviderComponent, ActivatableUIOpenAttemptEvent>(OnRuneDrawAttempt);
        SubscribeLocalEvent<RuneDrawerProviderComponent, BeforeActivatableUIOpenEvent>(BeforeRuneDraw);
        SubscribeLocalEvent<RuneDrawerProviderComponent, ListViewItemSelectedMessage>(OnRuneSelected);
        SubscribeLocalEvent<CultTeleportRuneProviderComponent, TeleportRunesListWindowItemSelectedMessage>(OnTeleportRuneSelected);

        SubscribeLocalEvent<CultRuneSummoningProviderComponent, SummonCultistListWindowItemSelectedMessage>(OnCultistSelected);

        // Rune drawing/erasing
        SubscribeLocalEvent<CultistComponent, CultDrawEvent>(OnDraw);
        SubscribeLocalEvent<CultistComponent, NameSelectorMessage>(OnChoose);
        SubscribeLocalEvent<CultRuneBaseComponent, InteractUsingEvent>(TryErase);
        SubscribeLocalEvent<CultRuneBaseComponent, CultEraseEvent>(OnErase);
        SubscribeLocalEvent<CultRuneBaseComponent, StartCollideEvent>(HandleCollision);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        InitializeBuffSystem();
        InitializeNarsie();
        InitializeSoulShard();
        InitializeConstructs();
        InitializeBarrierSystem();
        InitializeConstructsAbilities();
        InitializeActions();
        InitializeSpells();
    }

    private float _timeToDraw;

    private const string TeleportRunePrototypeId = "TeleportRune";
    private const string ApocalypseRunePrototypeId = "ApocalypseRune";
    private const string RitualDaggerPrototypeId = "RitualDagger";
    private const string RunicMetalPrototypeId = "CultRunicMetal";
    private const string SteelPrototypeId = "Steel";
    private const string NarsiePrototypeId = "Narsie";
    private const string CultBarrierPrototypeId = "CultBarrier";

    private bool _doAfterAlreadyStarted;

    private readonly SoundPathSpecifier _teleportInSound = new("/Audio/White/Cult/veilin.ogg");
    private readonly SoundPathSpecifier _teleportOutSound = new("/Audio/White/Cult/veilout.ogg");

    private readonly SoundPathSpecifier _magic = new("/Audio/White/Cult/magic.ogg");

    private readonly SoundPathSpecifier _apocRuneEndDrawing = new("/Audio/White/Cult/finisheddraw.ogg");
    private readonly SoundPathSpecifier _narsie40Sec = new("/Audio/White/Cult/40sec.ogg");

    private Entity<AudioComponent>? _narsieSummonningAudio;

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        CultRuneReviveComponent.ChargesLeft = 3;
    }

    /*
     * Rune draw start ----
     */

    private void OnRuneDrawAttempt(Entity<RuneDrawerProviderComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!HasComp<CultistComponent>(args.User))
            args.Cancel();
    }

    private void BeforeRuneDraw(Entity<RuneDrawerProviderComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        _ui.SetUiState(ent.Owner, ListViewSelectorUiKey.Key, new ListViewBUIState(ent.Comp.RunePrototypes, true));
    }

    private void OnRuneSelected(EntityUid uid, RuneDrawerProviderComponent component, ListViewItemSelectedMessage args)
    {
        var runePrototype = args.SelectedItem;
        var whoCalled = args.Actor;

        if (!HasComp<ActorComponent>(whoCalled))
            return;

        TryDraw(whoCalled, runePrototype);
    }

    private void TryDraw(EntityUid whoCalled, string runePrototype)
    {
        _timeToDraw = 4f;

        if (HasComp<CultBuffComponent>(whoCalled))
            _timeToDraw /= 2;

        if (!IsAllowedToDraw(whoCalled))
            return;

        if (runePrototype == ApocalypseRunePrototypeId)
        {
            if (!_mindSystem.TryGetMind(whoCalled, out _, out var mind) ||
                mind.Session is not { } playerSession)
                return;

            _euiManager.OpenEui(new ApocalypseRuneEui(whoCalled, _entityManager), playerSession);

            return;
        }

        var ev = new CultDrawEvent
        {
            Rune = runePrototype
        };

        var argsDoAfterEvent = new DoAfterArgs(_entityManager, whoCalled, _timeToDraw, ev, whoCalled)
        {
            BreakOnMove = true,
            NeedHand = true
        };

        if (!_doAfterSystem.TryStartDoAfter(argsDoAfterEvent))
            return;

        _audio.PlayPvs("/Audio/White/Cult/butcher.ogg", whoCalled, AudioParams.Default.WithMaxDistance(2f));
    }

    private void OnDraw(EntityUid uid, CultistComponent comp, CultDrawEvent args)
    {
        if (args.Cancelled)
            return;

        var howMuchBloodTake = -10;
        var rune = args.Rune;
        var user = args.User;

        if (HasComp<CultBuffComponent>(user))
            howMuchBloodTake /= 2;

        if (!TryComp<BloodstreamComponent>(user, out var bloodstreamComponent))
            return;

        _bloodstreamSystem.TryModifyBloodLevel(user, howMuchBloodTake, bloodstreamComponent, createPuddle: false);
        _audio.PlayPvs("/Audio/White/Cult/blood.ogg", user, AudioParams.Default.WithMaxDistance(2f));

        if (rune == TeleportRunePrototypeId)
        {
            if (!HasComp<ActorComponent>(user))
                return;

            _ui.OpenUi(user, NameSelectorUIKey.Key, user);

            return;
        }

        SpawnRune(user, rune);
    }

    private void OnChoose(EntityUid uid, CultistComponent component, NameSelectorMessage args)
    {
        if (!HasComp<ActorComponent>(uid))
            return;

        _ui.CloseUi(uid, NameSelectorUIKey.Key, uid);

        SpawnRune(uid, TeleportRunePrototypeId, true, args.Name);
    }

    //Erasing start

    private void TryErase(EntityUid uid, CultRuneBaseComponent component, InteractUsingEvent args)
    {
        if (TryComp<BibleComponent>(args.Used, out var bible) && HasComp<BibleUserComponent>(args.User))
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-erased-rune"), args.User, args.User);
            _audio.PlayPvs(bible.HealSoundPath, args.User);
            EntityManager.DeleteEntity(args.Target);
            return;
        }

        var entityPrototype = MetaData(args.Used).EntityPrototype;

        if (entityPrototype == null)
            return;

        var used = entityPrototype.ID;
        var user = args.User;
        var target = args.Target;
        var time = 3;

        if (used != RitualDaggerPrototypeId)
            return;

        if (!HasComp<CultistComponent>(user))
            return;

        if (HasComp<CultBuffComponent>(user))
            time /= 2;

        var netEntity = GetNetEntity(target);

        var ev = new CultEraseEvent
        {
            TargetEntityId = netEntity
        };

        var argsDoAfterEvent = new DoAfterArgs(_entityManager, user, time, ev, target)
        {
            BreakOnMove = true,
            NeedHand = true
        };

        if (_doAfterSystem.TryStartDoAfter(argsDoAfterEvent))
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-started-erasing-rune"), args.User, args.User);
        }
    }

    private void OnErase(EntityUid uid, CultRuneBaseComponent component, CultEraseEvent args)
    {
        if (args.Cancelled)
            return;

        var target = GetEntity(args.TargetEntityId);

        _entityManager.DeleteEntity(target);
        _popupSystem.PopupEntity(Loc.GetString("cult-erased-rune"), args.User, args.User);
    }

    private void HandleCollision(EntityUid uid, CultRuneBaseComponent component, ref StartCollideEvent args)
    {
        if (!TryComp<SolutionContainerManagerComponent>(args.OtherEntity, out var solution) ||
            !HasComp<VaporComponent>(args.OtherEntity) && !HasComp<SprayComponent>(args.OtherEntity))
        {
            return;
        }

        var cultRule = EntityManager.EntityQuery<CultRuleComponent>().FirstOrDefault();
        if (cultRule is null)
        {
            return;
        }

        var solutions = _solutionContainerSystem.EnumerateSolutions((args.OtherEntity, solution));

        if (solutions.Any(x => x.Solution.Comp.Solution.ContainsPrototype(cultRule.HolyWaterReagent)))
        {
            Del(uid);
        }
    }

    //Erasing end

    /*
     * Rune draw end ----
     */

    //------------------------------------------//

    /*
     * Base Start ----
     */

    private void OnActivate(EntityUid uid, CultRuneBaseComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!HasComp<CultistComponent>(args.User))
            return;

        var cultists = new HashSet<EntityUid>
        {
            args.User
        };

        if (component.InvokersMinCount > 1 || component.GatherInvokers)
            cultists = GatherCultists(uid, component.CultistGatheringRange);

        if (cultists.Count < component.InvokersMinCount)
        {
            _popupSystem.PopupEntity(Loc.GetString("not-enough-cultists"), args.User, args.User);
            return;
        }

        var ev = new CultRuneInvokeEvent(uid, args.User, cultists);
        RaiseLocalEvent(uid, ev);

        if (ev.Result)
        {
            OnAfterInvoke(uid, cultists, ev.InvokePhraseOverride);
        }
    }

    private void OnAfterInvoke(EntityUid rune, HashSet<EntityUid> cultists, string? invokePhraseOverride = null)
    {
        if (!TryComp<CultRuneBaseComponent>(rune, out var component))
            return;

        foreach (var cultist in cultists)
        {
            _chat.TrySendInGameICMessage(cultist, invokePhraseOverride ?? component.InvokePhrase,
                component.InvokeChatType, false, false, null, null, null, false);
        }
    }

    /*
     * Base End ----
     */

    //------------------------------------------//

    /*
     * Offering Rune START ----
     */

    private void OnInvokeOffering(EntityUid uid, CultRuneOfferingComponent component, CultRuneInvokeEvent args)
    {
        var targets =
            _lookup.GetEntitiesInRange(uid, component.RangeTarget, LookupFlags.Dynamic | LookupFlags.Sundries);

        targets.RemoveWhere(x =>
            !_entityManager.HasComponent<HumanoidAppearanceComponent>(x) || HasComp<CultistComponent>(x));

        if (targets.Count == 0)
            return;

        var victim = FindNearestTarget(uid, targets.ToList());

        if (victim == null)
            return;

        TryComp<MobStateComponent>(victim.Value, out var state);

        if (state == null)
            return;

        bool result;

        if (state.CurrentState != MobState.Dead)
        {
            // Выполнение действия в зависимости от условий - ахуеть а я не думал, это чатгпт писал?
            if (TryComp<MindContainerComponent>(victim.Value, out var mind) &&
                mind.Mind != null &&
                !HasComp<BibleUserComponent>(victim.Value) &&
                !HasComp<MindShieldComponent>(victim.Value) &&
                !IsTarget(mind.Mind.Value) &&
                _playerManager.TryGetSessionByEntity(victim.Value, out var session) )
            {
                result = Convert(uid, victim.Value, args.User, session, args.Cultists);
                args.InvokePhraseOverride = "Mah'weyh pleggh at e'ntrath!";
            }
            else
            {
                result = Sacrifice(uid, victim.Value, args.User, args.Cultists);
                args.InvokePhraseOverride = "Barhah hra zar'garis!";
            }
        }
        else // Жертва мертва, выполняется альтернативное действие
        {
            // Жертва мертва, выполняется альтернативное действие
            result = Sacrifice(uid, victim.Value, args.User, args.Cultists, true);
            args.InvokePhraseOverride = "Barhah hra zar'garis!";
        }

        args.Result = result;
    }

    private bool IsTarget(EntityUid mindId)
    {
        var target = _ruleSystem.GetTarget();
        if (target == null)
            return false;

        return mindId == target.Owner;
    }

    private bool Sacrifice(
        EntityUid rune,
        EntityUid target,
        EntityUid user,
        HashSet<EntityUid> cultists,
        bool isDead = false)
    {
        if (!TryComp<CultRuneOfferingComponent>(rune, out var offering))
            return false;

        var requiredCount = isDead ? offering.SacrificeDeadMinCount : offering.SacrificeMinCount;

        if (cultists.Count < requiredCount)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-sacrifice-not-enough-cultists"), user, user);
            return false;
        }

        if (!SpawnShard(target))
            _bodySystem.GibBody(target);

        AddChargesToReviveRune();
        return true;
    }

    private bool Convert(EntityUid rune, EntityUid target, EntityUid user, ICommonSession session, HashSet<EntityUid> cultists)
    {
        if (!TryComp<CultRuneOfferingComponent>(rune, out var offering))
            return false;

        if (cultists.Count < offering.ConvertMinCount)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-offering-rune-not-enough"), user, user);
            return false;
        }

        if (!_entityManager.TryGetComponent<ActorComponent>(target, out var actorComponent))
            return false;

        _stunSystem.TryStun(target, TimeSpan.FromSeconds(2f), false);

        _antag.ForceMakeAntag<CultRuleComponent>(session, "Cult");

        HealCultist(target);

        if (TryComp(target, out CuffableComponent? cuffs) && cuffs.Container.ContainedEntities.Count >= 1)
        {
            var lastAddedCuffs = cuffs.LastAddedCuffs;
            _cuffable.Uncuff(target, user, lastAddedCuffs);
        }

        _statusEffectsSystem.TryRemoveStatusEffect(target, "Muted");

        RemCompDeferred<BlightComponent>(target);

        return true;
    }

    /*
     * Offering Rune END ----
     */

    //------------------------------------------//

    /*
     * Buff Rune Start ----
     */

    private void OnInvokeBuff(EntityUid uid, CultRuneBuffComponent component, CultRuneInvokeEvent args)
    {
        var targets =
            _lookup.GetEntitiesInRange(uid, component.RangeTarget, LookupFlags.Dynamic | LookupFlags.Sundries);

        targets.RemoveWhere(x =>
            !_entityManager.HasComponent<HumanoidAppearanceComponent>(x) ||
            !_entityManager.HasComponent<CultistComponent>(x));

        if (targets.Count == 0)
            return;

        var victim = FindNearestTarget(uid, targets.ToList());

        if (victim == null)
            return;

        TryComp<MobStateComponent>(victim.Value, out var state);

        var result = false;

        if (state != null && state.CurrentState != MobState.Dead)
        {
            result = AddCultistBuff(victim.Value, args.User);
        }

        args.Result = result;
    }

    private bool AddCultistBuff(EntityUid target, EntityUid user)
    {
        if (TryComp<CultBuffComponent>(target, out var buff) && buff.BuffTime > buff.BuffLimit)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-buff-already-buffed"), user, user);
            return false;
        }

        buff = EnsureComp<CultBuffComponent>(target);
        buff.BuffTime = buff.StartingBuffTime;
        return true;
    }

    /*
     * Empower Rune End ----
     */

    //------------------------------------------//

    /*
     * Teleport rune start ----
     */

    private void OnInvokeTeleport(EntityUid uid, CultRuneTeleportComponent component, CultRuneInvokeEvent args)
    {
        var targets =
            _lookup.GetEntitiesInRange(uid, component.RangeTarget, LookupFlags.Dynamic | LookupFlags.Sundries);

        if (targets.Count == 0)
        {
            return;
        }

        args.Result = Teleport(uid, args.User, targets.ToList());
    }

    private bool Teleport(EntityUid rune, EntityUid user, List<EntityUid>? victims = null)
    {
        if (!OpenTeleportUi(user, rune))
            return false;

        _entityManager.EnsureComponent<CultTeleportRuneProviderComponent>(user, out var providerComponent);
        providerComponent.Targets = victims;
        providerComponent.BaseRune = rune;

        return true;
    }

    private bool OpenTeleportUi(EntityUid user, EntityUid? exceptRune = null)
    {
        var runesQuery = EntityQueryEnumerator<CultRuneTeleportComponent>();
        var list = new List<int>();
        var labels = new List<string>();

        while (runesQuery.MoveNext(out var runeUid, out var teleportComponent))
        {
            if (teleportComponent.Label == null)
                continue;

            if (runeUid == exceptRune)
                continue;

            if (!int.TryParse(runeUid.ToString(), out var intValue))
                continue;

            list.Add(intValue);
            labels.Add(teleportComponent.Label);
        }

        if (!TryComp<ActorComponent>(user, out var actorComponent))
            return false;

        if (list.Count == 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-teleport-rune-not-found"), user, user);
            return false;
        }

        _ui.SetUiState(user, RuneTeleporterUiKey.Key, new TeleportRunesListWindowBUIState(list, labels));

        _ui.TryToggleUi(user, RuneTeleporterUiKey.Key, actorComponent.PlayerSession);
        return true;
    }

    private void OnTeleportRuneSelected(
        EntityUid uid,
        CultTeleportRuneProviderComponent component,
        TeleportRunesListWindowItemSelectedMessage args)
    {
        var targets = component.Targets;
        var user = args.Actor;
        var selectedRune = new EntityUid(args.SelectedItem);
        var baseRune = component.BaseRune;

        if (targets is null || targets.Count == 0)
            return;

        if (baseRune == null)
            return;

        if (!TryComp<TransformComponent>(selectedRune, out var xFormSelected) ||
            !TryComp<TransformComponent>(baseRune, out var xFormBase))
            return;

        foreach (var target in targets)
        {
            StopPulling(target);

            _xform.SetCoordinates(target, xFormSelected.Coordinates);
        }

        //Play tp sound
        _audio.PlayPvs(_teleportInSound, xFormSelected.Coordinates);
        _audio.PlayPvs(_teleportOutSound, xFormBase.Coordinates);

        if (HasComp<CultTeleportRuneProviderComponent>(user))
        {
            RemComp<CultTeleportRuneProviderComponent>(user);
        }
    }

    /*
     * Teleport rune end ----
     */

    //------------------------------------------//

    /*
     * Apocalypse rune start ----
     */

    private void OnInvokeApocalypse(EntityUid uid, CultRuneApocalypseComponent component, CultRuneInvokeEvent args)
    {
        args.Result = TrySummonNarsie(args.User, args.Cultists, component);
    }

    private bool TrySummonNarsie(EntityUid user, HashSet<EntityUid> cultists, CultRuneApocalypseComponent component)
    {
        var canSummon = _ruleSystem.CanSummonNarsie();

        if (!canSummon)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-narsie-not-completed-tasks"), user, user);
            return false;
        }

        if (cultists.Count < component.SummonMinCount)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-narsie-summon-not-enough"), user, user);
            return false;
        }

        if (_doAfterAlreadyStarted)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-narsie-already-summoning"), user, user);
            return false;
        }

        if (!TryComp<DoAfterComponent>(user, out var doAfterComponent))
        {
            if (doAfterComponent is { AwaitedDoAfters.Count: >= 1 })
            {
                _popupSystem.PopupEntity(Loc.GetString("cult-narsie-summon-do-after"), user, user);
                return false;
            }
        }

        var ev = new SummonNarsieDoAfterEvent();

        var argsDoAfterEvent = new DoAfterArgs(_entityManager, user, TimeSpan.FromSeconds(40), ev, user)
        {
            BreakOnMove = true
        };

        if (!_doAfterSystem.TryStartDoAfter(argsDoAfterEvent))
            return false;

        _popupSystem.PopupEntity(Loc.GetString("cult-stay-still"), user, user, PopupType.LargeCaution);

        _doAfterAlreadyStarted = true;

        _chat.DispatchGlobalAnnouncement(Loc.GetString("cult-ritual-started"), "CULT", false,
            colorOverride: Color.DarkRed);

        _narsieSummonningAudio = _audio.PlayGlobal(_narsie40Sec, Filter.Broadcast(), false, AudioParams.Default.WithLoop(true).WithVolume(0.15f));

        return true;
    }

    private void NarsieSpawn(EntityUid uid, CultistComponent component, SummonNarsieDoAfterEvent args)
    {
        _doAfterAlreadyStarted = false;

        _audio.Stop(_narsieSummonningAudio?.Owner, _narsieSummonningAudio?.Comp);

        if (args.Cancelled)
        {
            _chat.DispatchGlobalAnnouncement(Loc.GetString("cult-ritual-prevented"), "CULT", false,
                colorOverride: Color.DarkRed);

            return;
        }

        var transform = CompOrNull<TransformComponent>(args.User)?.Coordinates;
        if (transform == null)
            return;

        var ev = new CultNarsieSummoned();
        RaiseLocalEvent(ev);

        _entityManager.SpawnEntity(NarsiePrototypeId, transform.Value);

        //_chat.DispatchGlobalAnnouncement(Loc.GetString("cult-narsie-summoned"), "CULT", true, _apocRuneEndDrawing,
        //    colorOverride: Color.DarkRed);
    }

    /*
     * Apocalypse rune end ----
     */

    //------------------------------------------//

    /*
     * Revive rune start ----
     */

    private void OnInvokeRevive(EntityUid uid, CultRuneReviveComponent component, CultRuneInvokeEvent args)
    {
        var targets =
            _lookup.GetEntitiesInRange(uid, component.RangeTarget, LookupFlags.Dynamic | LookupFlags.Sundries);

        targets.RemoveWhere(x => !_entityManager.HasComponent<HumanoidAppearanceComponent>(x));

        if (targets.Count == 0)
            return;

        var victim = FindNearestTarget(uid, targets.ToList());

        if (victim == null)
            return;

        if (_mobState.IsAlive(victim.Value))
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-revive-rune-already-alive"), args.User, args.User);
            return;
        }

        var result = Revive(victim.Value, args.User);

        args.Result = result;
    }

    private bool Revive(EntityUid target, EntityUid user)
    {
        if (HasComp<CultistComponent>(target))
        {
            if (CultRuneReviveComponent.ChargesLeft == 0)
            {
                _popupSystem.PopupEntity(Loc.GetString("cult-revive-rune-no-charges"), user, user);
                return false;
            }

            CultRuneReviveComponent.ChargesLeft--;

            _entityManager.EventBus.RaiseLocalEvent(target, new RejuvenateEvent());
        }
        else
        {
            if (!TryComp(target, out DamageableComponent? damageable) ||
                !TryComp(target, out MobThresholdsComponent? threshold) ||
                !TryComp(target, out MobStateComponent? mobState))
                return false;

            if (!_mobState.IsDead(target, mobState))
            {
                _popupSystem.PopupEntity(Loc.GetString("cult-revive-rune-already-alive"), user, user);
                return false;
            }

            var airlossGroup = _prototypeManager.Index<DamageGroupPrototype>("Airloss");

            var deadThreshold = _thresholdSystem.GetThresholdForState(target, MobState.Dead, threshold);

            if (damageable.Damage.TryGetDamageInGroup(airlossGroup, out var toHeal))
            {
                var afterHeal = damageable.TotalDamage - toHeal;
                if (deadThreshold <= afterHeal)
                {
                    _popupSystem.PopupEntity(Loc.GetString("cult-revive-rune-too-damaged"), user, user);
                    return false;
                }

                var asphyxType = _prototypeManager.Index<DamageTypePrototype>("Asphyxiation");
                var bloodlossType = _prototypeManager.Index<DamageTypePrototype>("Bloodloss");

                var heal = new Action<DamageTypePrototype>(type =>
                {
                    if (!damageable.Damage.DamageDict.TryGetValue(type.ID, out var damage))
                        return;

                    _damageableSystem.TryChangeDamage(target, new DamageSpecifier(type, -damage));
                });

                heal(asphyxType);
                heal(bloodlossType);
            }

            if (damageable.TotalDamage < deadThreshold)
                _mobState.ChangeMobState(target, MobState.Critical, mobState, user);
        }

        EntityUid? transferTo = null;

        if (!_mindSystem.TryGetMind(target, out var mindId, out var mind))
        {
            if (!TryComp<CultistComponent>(target, out var cultist) || cultist.OriginalMind == null)
                return true;

            (mindId, mind) = cultist.OriginalMind.Value;

            transferTo = target;
        }

        if (mind.Session is not { } playerSession)
            return true;

        // notify them they're being revived.
        if (mind.CurrentEntity != target)
        {
            _euiManager.OpenEui(new ReturnToBodyEui(mind, _mindSystem, mindId, transferTo), playerSession);
        }
        return true;
    }

    /*
     * Revive rune end ----
     */

    //------------------------------------------//

    /*
     * Barrier rune start ----
     */

    private void OnInvokeBarrier(EntityUid uid, CultRuneBarrierComponent component, CultRuneInvokeEvent args)
    {
        args.Result = SpawnBarrier(args.Rune);
    }

    private bool SpawnBarrier(EntityUid rune)
    {
        var transform = CompOrNull<TransformComponent>(rune)?.Coordinates;

        if (transform == null)
            return false;

        _entityManager.SpawnEntity(CultBarrierPrototypeId, transform.Value);
        _entityManager.DeleteEntity(rune);

        return true;
    }

    /*
     * Barrier rune end ----
     */

    //------------------------------------------//

    /*
     * Summoning rune start ----
     */

    private void OnInvokeSummoning(EntityUid uid, CultRuneSummoningComponent component, CultRuneInvokeEvent args)
    {
        args.Result = Summon(uid, args.User, args.Cultists, component);
    }

    private bool Summon(
        EntityUid rune,
        EntityUid user,
        HashSet<EntityUid> cultistHashSet,
        CultRuneSummoningComponent component)
    {
        var cultistsQuery = EntityQueryEnumerator<CultistComponent>();
        var list = new List<int>();
        var labels = new List<string>();

        if (cultistHashSet.Count < component.SummonMinCount)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-summon-rune-need-minimum-cultists"), user, user);
            return false;
        }

        while (cultistsQuery.MoveNext(out var cultistUid, out _))
        {
            var meta = MetaData(cultistUid);
            if (cultistHashSet.Contains(cultistUid))
                continue;

            if (!int.TryParse(cultistUid.ToString(), out var intValue))
                continue;

            list.Add(intValue);
            labels.Add(meta.EntityName);
        }

        if (!TryComp<ActorComponent>(user, out var actorComponent))
            return false;

        if (list.Count == 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-cultists-not-found"), user, user);
            return false;
        }

        _entityManager.EnsureComponent<CultRuneSummoningProviderComponent>(user, out var providerComponent);
        providerComponent.BaseRune = rune;

        _ui.SetUiState(user, SummonCultistUiKey.Key, new SummonCultistListWindowBUIState(list, labels));

        _ui.TryToggleUi(user, SummonCultistUiKey.Key, actorComponent.PlayerSession);
        return true;
    }

    private void OnCultistSelected(
        EntityUid uid,
        CultRuneSummoningProviderComponent component,
        SummonCultistListWindowItemSelectedMessage args)
    {
        var user = args.Actor;
        var target = new EntityUid(args.SelectedItem);
        var baseRune = component.BaseRune;

        if (!TryComp<PullableComponent>(target, out var pullableComponent))
            return;

        if (!TryComp<CuffableComponent>(target, out var cuffableComponent))
            return;

        if (baseRune == null)
            return;

        if (!TryComp<TransformComponent>(baseRune, out var xFormBase))
            return;

        var isCuffed = cuffableComponent.CuffedHandCount > 0;
        var isPulled = pullableComponent.BeingPulled;

        if (isPulled)
        {
            _popupSystem.PopupEntity("Его кто-то держит!", user);
            return;
        }

        if (isCuffed)
        {
            _popupSystem.PopupEntity("Он в наручниках!", user);
            return;
        }

        StopPulling(target, false);

        _xform.SetCoordinates(target, xFormBase.Coordinates);

        _audio.PlayPvs(_teleportInSound, xFormBase.Coordinates);

        if (HasComp<CultRuneSummoningProviderComponent>(user))
        {
            RemComp<CultRuneSummoningProviderComponent>(user);
        }
    }

    /*
     * Summoning rune end ----
     */

    //------------------------------------------//

    /*
     * BloodBoil rune start ----
     */

    private void OnInvokeBloodBoil(EntityUid uid, CultRuneBloodBoilComponent component, CultRuneInvokeEvent args)
    {
        args.Result = PrepareShoot(uid, args.User, args.Cultists, 1.0f, component);
    }

    private bool PrepareShoot(
        EntityUid rune,
        EntityUid user,
        HashSet<EntityUid> cultists,
        float severity,
        CultRuneBloodBoilComponent component)
    {
        cultists = cultists.Where(HasComp<CultistComponent>).ToHashSet(); // Prevent constructs from using the rune

        if (cultists.Count < component.SummonMinCount)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-blood-boil-rune-need-minimum"), user, user);
            return false;
        }

        var xformQuery = GetEntityQuery<TransformComponent>();
        var xform = xformQuery.GetComponent(rune);

        var inRange = _lookup.GetEntitiesInRange(rune, component.ProjectileRange * severity, LookupFlags.Dynamic);
        inRange.RemoveWhere(x =>
            !_entityManager.HasComponent<HumanoidAppearanceComponent>(x) ||
            _entityManager.HasComponent<CultistComponent>(x));

        var list = inRange.ToList();

        if (list.Count == 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-blood-boil-rune-no-targets"), user, user);
            return false;
        }

        _random.Shuffle(list);

        var bloodCost = 120 / cultists.Count;

        foreach (var cultist in cultists)
        {
            if (!TryComp<BloodstreamComponent>(cultist, out var bloodstreamComponent) ||
                bloodstreamComponent.BloodSolution is null)
            {
                return false;
            }

            if (bloodstreamComponent.BloodSolution.Value.Comp.Solution.Volume < bloodCost)
            {
                _popupSystem.PopupEntity(Loc.GetString("cult-blood-boil-rune-no-blood"), user, user);
                return false;
            }

            _bloodstreamSystem.TryModifyBloodLevel(cultist, -bloodCost, bloodstreamComponent);
        }

        var projectileCount =
            (int) MathF.Round(MathHelper.Lerp(component.MinProjectiles, component.MaxProjectiles, severity));

        while (projectileCount > 0)
        {
            var target = _random.Pick(list);
            var targetCoords = xformQuery.GetComponent(target).Coordinates.Offset(_random.NextVector2(0.5f));
            var flammable = GetEntityQuery<FlammableComponent>();

            if (!flammable.TryGetComponent(target, out var fl))
                continue;

            fl.FireStacks += 1;

            _flammableSystem.Ignite(target, target);

            Shoot(
                rune,
                component,
                xform.Coordinates,
                targetCoords,
                severity);

            projectileCount--;
        }

        _audio.PlayPvs(_magic, rune, AudioParams.Default.WithMaxDistance(2f));

        return true;
    }

    private void Shoot(
        EntityUid uid,
        CultRuneBloodBoilComponent component,
        EntityCoordinates coords,
        EntityCoordinates targetCoords,
        float severity)
    {
        var mapPos = coords.ToMap(EntityManager, _xform);

        var spawnCoords = _mapManager.TryFindGridAt(mapPos, out var gridUid, out _)
            ? coords.WithEntityId(gridUid, EntityManager)
            : new(_mapManager.GetMapEntityId(mapPos.MapId), mapPos.Position);

        var ent = Spawn(component.ProjectilePrototype, spawnCoords);
        var direction = targetCoords.ToMapPos(EntityManager, _xform) - mapPos.Position;

        if (!TryComp<ProjectileComponent>(ent, out var comp))
            return;

        comp.Damage *= severity;

        _gunSystem.ShootProjectile(ent, direction, Vector2.Zero, uid, uid, component.ProjectileSpeed);
    }

    /*
     * BloodBoil rune end ----
     */

    //------------------------------------------//

    /*
     * Empower rune start ----
     */

    private void OnActiveInWorld(EntityUid uid, CultEmpowerComponent component, CultRuneInvokeEvent args)
    {
        if (!component.IsRune || !TryComp<CultistComponent>(args.User, out _) || !HasComp<ActorComponent>(args.User))
            return;

        args.Result = !_ui.HasUi(uid, CultEmpowerUiKey.Key) &&
                      _ui.TryOpenUi(uid, CultEmpowerUiKey.Key, args.User);
    }

    private void OnUseInHand(EntityUid uid, CultEmpowerComponent component, UseInHandEvent args)
    {
        if (!HasComp<CultistComponent>(args.User) || !HasComp<ActorComponent>(args.User))
            return;

        _ui.OpenUi(uid, CultEmpowerUiKey.Key, args.User);
    }

    private void OnEmpowerSelected(EntityUid uid, CultEmpowerComponent component, CultEmpowerSelectedBuiMessage args)
    {
        if (!TryComp<CultistComponent>(args.Actor, out var comp))
            return;

        var action = CultistComponent.CultistActions.FirstOrDefault(x => x.Equals(args.ActionType));

        if (action == null)
            return;

        if (component.IsRune)
        {
            if (comp.SelectedEmpowers.Count >= component.MaxAllowedCultistActions)
            {
                _popupSystem.PopupEntity(Loc.GetString("cult-too-much-empowers"), uid);
                return;
            }

            comp.SelectedEmpowers.Add(GetNetEntity(_actionsSystem.AddAction(args.Actor, action)));
            Dirty(args.Actor, comp);
        }
        else if (comp.SelectedEmpowers.Count < component.MinRequiredCultistActions)
        {
            comp.SelectedEmpowers.Add(GetNetEntity(_actionsSystem.AddAction(args.Actor, action)));
            Dirty(args.Actor, comp);
        }
    }

    /*
     * Empower rune end ----
     */

    //------------------------------------------//

    /*
     * Helpers Start ----
     */

    private EntityUid? FindNearestTarget(EntityUid uid, List<EntityUid> targets)
    {
        if (!TryComp<TransformComponent>(uid, out var runeTransform))
            return null;

        var range = 999f;
        EntityUid? victim = null;

        foreach (var target in targets)
        {
            if (!TryComp<TransformComponent>(target, out var targetTransform))
                continue;

            runeTransform.Coordinates.TryDistance(_entityManager, targetTransform.Coordinates, out var newRange);

            if (newRange < range)
            {
                range = newRange;
                victim = target;
            }
        }

        return victim;
    }

    private HashSet<EntityUid> GatherCultists(EntityUid uid, float range)
    {
        var entities = _lookup.GetEntitiesInRange(uid, range, LookupFlags.Dynamic);
        entities.RemoveWhere(x => !HasComp<CultistComponent>(x) && !HasComp<ConstructComponent>(x));

        return entities;
    }

    private void SpawnRune(EntityUid uid, string? rune, bool teleportRune = false, string? label = null)
    {
        var transform = CompOrNull<TransformComponent>(uid)?.Coordinates;

        if (transform == null)
            return;

        if (rune == null)
            return;

        if (teleportRune)
        {
            var teleportRuneEntity = _entityManager.SpawnEntity(rune, transform.Value);
            _xform.AttachToGridOrMap(teleportRuneEntity);

            TryComp<CultRuneTeleportComponent>(teleportRuneEntity, out var sex);
            {
                if (sex == null)
                    return;

                label = string.IsNullOrEmpty(label) ? Loc.GetString("cult-teleport-rune-default-label") : label;

                if (label.Length > 18)
                {
                    label = label.Substring(0, 18);
                }

                sex.Label = label;
            }

            return;
        }

        var damage = 10;

        if (rune == ApocalypseRunePrototypeId)
        {
            if (!TryComp(uid, out TransformComponent? transComp))
            {
                return;
            }

            damage = 40;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("cult-narsie-summon-drawn-position",
                    ("location", FormattedMessage.RemoveMarkup(_navMap.GetNearestBeaconString((uid, transComp))))),
                "CULT", true, _apocRuneEndDrawing, colorOverride: Color.DarkRed);
        }

        var damageSpecifier = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Slash"), damage);
        _damageableSystem.TryChangeDamage(uid, damageSpecifier, true, false);

        _xform.AttachToGridOrMap(_entityManager.SpawnEntity(rune, transform.Value));
    }

    private bool SpawnShard(EntityUid target)
    {
        if (!TryComp<MindContainerComponent>(target, out var mindComponent))
            return false;

        var transform = CompOrNull<TransformComponent>(target)?.Coordinates;

        if (transform == null)
            return false;

        if (mindComponent.Mind.HasValue == false)
            return false;

        var shard = _entityManager.SpawnEntity("SoulShard", transform.Value);

        if (shard.Valid == false)
            return false;

        _mindSystem.TransferTo(mindComponent.Mind.Value, shard);

        _bodySystem.GibBody(target);

        return true;
    }

    private void AddChargesToReviveRune(uint amount = 1)
    {
        CultRuneReviveComponent.ChargesLeft += amount;
    }

    private bool IsAllowedToDraw(EntityUid uid)
    {
        var transform = Transform(uid);
        var gridUid = transform.GridUid;
        var tile = transform.Coordinates.GetTileRef();

        if (!gridUid.HasValue)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-cant-draw-rune"), uid, uid);
            return false;
        }

        if (!tile.HasValue)
        {
            _popupSystem.PopupEntity(Loc.GetString("cult-cant-draw-rune"), uid, uid);
            return false;
        }

        return true;
    }

    private void HealCultist(EntityUid player)
    {
        var damageSpecifier = _prototypeManager.Index<DamageGroupPrototype>("Brute");
        var damageSpecifier2 = _prototypeManager.Index<DamageGroupPrototype>("Burn");

        _damageableSystem.TryChangeDamage(player, new DamageSpecifier(damageSpecifier, -40));
        _damageableSystem.TryChangeDamage(player, new DamageSpecifier(damageSpecifier2, -40));
    }

    private void StopPulling(EntityUid target, bool checkPullable = true)
    {
        // break pulls before portal enter so we dont break shit
        if (checkPullable && TryComp<PullableComponent>(target, out var pullable) && pullable.BeingPulled)
        {
            _pulling.TryStopPull(target, pullable);
        }

        if (_pulling.TryGetPulledEntity(target, out var pulledEntity)
            && TryComp(pulledEntity, out PullableComponent? subjectPulling))
        {
            _pulling.TryStopPull(pulledEntity.Value, subjectPulling);
        }
    }

    /*
     * Helpers End ----
     */
}
