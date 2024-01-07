using Content.Shared.Damage.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.White.Ryodan.Components;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Shared.White.Ryodan.Systems;

public sealed class RyodanKatanaSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StaminaSystem _staminaSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RyodanKatanaComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<RyodanKatanaComponent, GotUnequippedHandEvent>(OnUnequipped);

        SubscribeLocalEvent<RyodanKatanaUserComponent, ProjectileReflectAttemptEvent>(ReflectAttempt);

        SubscribeLocalEvent<RyodanKatanaComponent, DashAttemptEvent>(OnDash);
        SubscribeLocalEvent<RyodanKatanaComponent, ThrowingStarSpawnAttemptEvent>(OnThrowingStar);
    }

    private void OnEquipped(EntityUid uid, RyodanKatanaComponent component, GotEquippedHandEvent args)
    {
        EnsureComp<RyodanKatanaUserComponent>(args.User);
    }

    private void OnUnequipped(EntityUid uid, RyodanKatanaComponent component, GotUnequippedHandEvent args)
    {
        RemComp<RyodanKatanaUserComponent>(args.User);
    }

    private void ReflectAttempt(EntityUid uid, RyodanKatanaUserComponent component, ref ProjectileReflectAttemptEvent args)
    {
        if (!_staminaSystem.TryTakeStamina(uid, 5))
        {
            args.Cancelled = true;
        }
    }

    private void OnDash(EntityUid uid, RyodanKatanaComponent component, DashAttemptEvent args)
    {
        if (HasComp<RyodanComponent>(args.User))
            return;

        _popup.PopupEntity("You are not a member of Ryodan Clan!", args.User);

        args.Cancel();
    }

    private void OnThrowingStar(EntityUid uid, RyodanKatanaComponent component, ThrowingStarSpawnAttemptEvent args)
    {
        if (HasComp<RyodanComponent>(args.User))
            return;

        _popup.PopupEntity("You are not a member of Ryodan Clan!", args.User);

        args.Cancel();
    }
}
