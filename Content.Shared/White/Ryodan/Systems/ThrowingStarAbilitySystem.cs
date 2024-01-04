using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.White.Ryodan.Components;
using Robust.Shared.Audio;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.White.Ryodan.Systems;

/// <summary>
/// Handles ThrowingStar spawn logic.
/// </summary>
public sealed class ThrowingStarAbilitySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrowingStarAbilityComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<ThrowingStarAbilityComponent, ThrowingStarEvent>(OnStar);
    }

    private void OnGetItemActions(EntityUid uid, ThrowingStarAbilityComponent comp, GetItemActionsEvent args)
    {
        var ev = new AddThrowingStarActionEvent(args.User);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return;

        if (!_prototypeManager.TryIndex<InstantActionPrototype>(comp.ActionId, out var action))
            return;

        args.Actions.Add(new InstantAction(action));
    }

    private void OnStar(EntityUid uid, ThrowingStarAbilityComponent comp, ThrowingStarEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var user = args.Performer;
        args.Handled = true;

        var ev = new ThrowingStarSpawnAttemptEvent(user);
        RaiseLocalEvent(uid, ev);
        if (ev.Cancelled)
            return;

        TryComp<LimitedChargesComponent>(uid, out var charges);
        if (_charges.IsEmpty(uid, charges))
        {
            _popup.PopupEntity(Loc.GetString("dash-ability-no-charges", ("item", uid)), user, user);
            return;
        }

        if (_net.IsServer)
        {
            GiveItem(args.Performer);
        }

        _audio.PlayPvs(comp.ActionSound, user, AudioParams.Default);

        if (charges != null)
            _charges.UseCharge(uid, charges);
    }

    private void GiveItem(EntityUid player)
    {
        var transform = CompOrNull<TransformComponent>(player);

        if (transform == null)
            return;

        if (!HasComp<HandsComponent>(player))
            return;

        var weaponEntity = EntityManager.SpawnEntity("ThrowingStarRyodan", transform.Coordinates);

        _hands.TryPickupAnyHand(player, weaponEntity);
    }
}

public sealed class AddThrowingStarActionEvent : CancellableEntityEventArgs
{
    public EntityUid User;

    public AddThrowingStarActionEvent(EntityUid user)
    {
        User = user;
    }
}

public sealed class ThrowingStarSpawnAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid User;

    public ThrowingStarSpawnAttemptEvent(EntityUid user)
    {
        User = user;
    }
}
