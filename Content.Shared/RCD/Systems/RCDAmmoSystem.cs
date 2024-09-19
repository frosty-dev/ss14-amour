using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Content.Shared.Stacks;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared.RCD.Systems;

public sealed class RCDAmmoSystem : EntitySystem
{
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly INetManager _netMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RCDAmmoComponent, ComponentInit>(OnInit); // WD edit

        SubscribeLocalEvent<RCDAmmoComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<RCDAmmoComponent, AfterInteractEvent>(OnAfterInteract);
    }

    // WD edit start
    private void OnInit(EntityUid uid, RCDAmmoComponent rcdAmmoComponent, ComponentInit _)
    {
        if (TryComp<StackComponent>(uid, out var stackComponent))
            rcdAmmoComponent.Charges = (int) (stackComponent.Count * rcdAmmoComponent.ChargeCountModifier);
        else
            rcdAmmoComponent.Charges = (int) (rcdAmmoComponent.Charges * rcdAmmoComponent.ChargeCountModifier);

    }
    // WD edit end

    private void OnExamine(EntityUid uid, RCDAmmoComponent comp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!comp.CanBeExamined) // WD edit
            return;

        var examineMessage = Loc.GetString("rcd-ammo-component-on-examine", ("charges", comp.Charges));
        args.PushText(examineMessage);
    }

    private void OnAfterInteract(EntityUid uid, RCDAmmoComponent comp, AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || !_timing.IsFirstTimePredicted)
            return;

        if (args.Target is not { Valid: true } target ||
            !HasComp<RCDComponent>(target) ||
            !TryComp<LimitedChargesComponent>(target, out var charges))
            return;

        var user = args.User;
        args.Handled = true;

        // WD edit start
        TryComp<StackComponent>(uid, out var stackComponent);
        if (stackComponent != null)
        {
            var realValue = (int) (stackComponent.Count * comp.ChargeCountModifier);
            comp.Charges = realValue;
            if (realValue == 0)
            {
                _popup.PopupClient(Loc.GetString("rcd-ammo-component-after-interact-not-enough"), target, user);
                return;
            }
        }
        // WD edit end

        var count = Math.Min(charges.MaxCharges - charges.Charges, comp.Charges);
        if (count <= 0)
        {
            _popup.PopupClient(Loc.GetString("rcd-ammo-component-after-interact-full"), target, user);
            return;
        }

        _popup.PopupClient(Loc.GetString("rcd-ammo-component-after-interact-refilled"), target, user);

        // WD edit start
        if (stackComponent != null)
        {
            var spent = (int) (count / comp.ChargeCountModifier) == 0 ? 1 : (int) (count / comp.ChargeCountModifier);
            _stack.SetCount(uid, stackComponent.Count - spent);
        }
        // WD edit end

        _charges.AddCharges(target, count, charges);
        comp.Charges -= count;
        Dirty(uid, comp);

        // prevent having useless ammo with 0 charges
        if (comp.Charges <= 0 && stackComponent == null && _netMan.IsServer)
            QueueDel(uid);
    }
}
