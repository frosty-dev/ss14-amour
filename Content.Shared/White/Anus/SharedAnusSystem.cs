using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.White.Anus;

public abstract class SharedAnusSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AnusComponent,IsEquippingAttemptEvent>(OnEquipping);
        SubscribeLocalEvent<AnusComponent,IsUnequippingAttemptEvent>(OnUnequip);
        SubscribeLocalEvent<AnusComponent,ComponentStartup>(OnStartup);
        SubscribeLocalEvent<AnusComponent,GetVerbsEvent<AlternativeVerb>>(OnVerb);
        SubscribeLocalEvent<AnusComponent,InspectAnusDoAfterEvent>(OnInspect);
        SubscribeLocalEvent<AnusComponent,InsertToAnusDoAfterEvent>(OnInsert);
    }

    private void OnInsert(EntityUid uid, AnusComponent component, InsertToAnusDoAfterEvent args)
    {
        if(args is { Used: not null, Cancelled: false })
            InsertToAnus(uid,args.Used.Value,component);
    }

    private void OnInspect(EntityUid uid, AnusComponent component, InspectAnusDoAfterEvent args)
    {
        if(!args.Cancelled)
            InspectAnus(uid, args.User, component);
    }

    private void OnVerb(EntityUid uid, AnusComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if(!HasAccessToAnus(uid) || !args.CanAccess || !args.CanInteract ||
           !TryComp<HandsComponent>(args.User,out var hands) || hands.ActiveHand == null)
            return;

        if(!hands.ActiveHand.HeldEntity.HasValue)
        {
            args.Verbs.Add(new()
            {
                Act = () =>
                {
                    var doAfterArgs = new DoAfterArgs(args.User, TimeSpan.FromSeconds(6), new InspectAnusDoAfterEvent(),
                        uid,uid)
                    {
                        BreakOnTargetMove = true,
                        BreakOnUserMove = true,
                        BreakOnDamage = true
                    };

                    if(_net.IsServer)
                        _popup.PopupEntity(Loc.GetString("anus-inspecting"),uid,uid,PopupType.Medium);
                    _doAfter.TryStartDoAfter(doAfterArgs);
                },
                Text = Loc.GetString("anus-inspect")
            });
        }
        else
        {
            args.Verbs.Add(new()
            {
                Act = () =>
                {
                    var doAfterArgs = new DoAfterArgs(args.User, TimeSpan.FromSeconds(6),
                        new InsertToAnusDoAfterEvent(), uid, uid, hands.ActiveHand.HeldEntity.Value)
                    {
                        BreakOnTargetMove = true,
                        BreakOnUserMove = true,
                        BreakOnDamage = true
                    };

                    if(_net.IsServer)
                        _popup.PopupEntity(Loc.GetString("anus-inserting"),uid,uid,PopupType.Medium);
                    _doAfter.TryStartDoAfter(doAfterArgs);
                },
                Text = Loc.GetString("anus-insert")
            });
        }

    }

    private void OnUnequip(EntityUid uid, AnusComponent component, IsUnequippingAttemptEvent args)
    {
        if(args.Slot != AnusComponent.SlotName)
            return;
        args.Reason = Loc.GetString("anus-no-access");

        if(!HasAccessToAnus(args.UnEquipTarget))
        {
            args.Cancel();
            return;
        }

        if (_net.IsServer)
        {
            args.Cancel();

            var doAfterArgs = new DoAfterArgs(args.UnEquipTarget, TimeSpan.FromSeconds(6), new InspectAnusDoAfterEvent(),
                args.UnEquipTarget,args.UnEquipTarget)
            {
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                BreakOnDamage = true
            };
            _doAfter.TryStartDoAfter(doAfterArgs);
        }
    }

    private void OnEquipping(EntityUid uid, AnusComponent component, IsEquippingAttemptEvent args)
    {
        if(args.Slot != AnusComponent.SlotName)
            return;
        args.Reason = Loc.GetString("anus-no-access");

        if (!HasAccessToAnus(args.EquipTarget))
        {
            args.Cancel();
            return;
        }

        if (_net.IsServer)
        {
            args.Cancel();

            var doAfterArgs = new DoAfterArgs(args.EquipTarget, TimeSpan.FromSeconds(6),
                new InsertToAnusDoAfterEvent(), args.EquipTarget, args.EquipTarget, args.Equipment)
            {
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                BreakOnDamage = true
            };

            _doAfter.TryStartDoAfter(doAfterArgs);
        }
    }

    private void OnStartup(EntityUid uid, AnusComponent component, ComponentStartup args)
    {
        if (!_inventory.TryGetSlotContainer(uid,AnusComponent.SlotName,out var anusSlot,out var anus))
        {
            RemComp<AnusComponent>(uid);
            return;
        }

        component.NextMoanTime = Timing.CurTime;
        component.AnusSlot = anusSlot;
    }

    public bool HasAccessToAnus(EntityUid uid, AnusComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;
        return !(_inventory.TryGetSlotEntity(uid, "suitstorage", out _) ||
                 _inventory.TryGetSlotEntity(uid, "outerClothing", out _) ||
                 _inventory.TryGetSlotEntity(uid, "jumpsuit", out _) ||
                 _inventory.TryGetSlotEntity(uid, "underwearb", out _));
    }

    public bool InspectAnus(EntityUid uid,EntityUid inspector, AnusComponent? component = null)
    {
        if (!Resolve(uid, ref component) || !HasAccessToAnus(uid))
            return false;

        if (!_inventory.TryGetSlotEntity(uid, AnusComponent.SlotName, out var entityUid))
            return false;

        if (!TryComp<HandsComponent>(inspector, out var hands) || hands.ActiveHand == null)
            return false;

        return _hands.TryPickup(inspector,entityUid.Value,hands.ActiveHand);
    }

    public void InsertToAnus(EntityUid uid,EntityUid entityUid, AnusComponent? component = null)
    {
        if (!Resolve(uid, ref component) || !HasAccessToAnus(uid))
            return;

        component.AnusSlot.Insert(entityUid);
    }
}


[Serializable]
[NetSerializable]
public sealed class InspectAnusDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable]
[NetSerializable]
public sealed class InsertToAnusDoAfterEvent : SimpleDoAfterEvent
{
}
