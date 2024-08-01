using Content.Server._White.Carrying;
using Content.Server.DoAfter;
using Content.Shared.Verbs;
using Content.Shared.Item;
using Content.Server.Storage.EntitySystems;
using Content.Server.Item;
using Content.Server.Popups;
using Content.Server.Resist;
using Content.Shared._White.Carrying;
using Content.Shared._White.Item.PseudoItem;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Resist;
using Content.Shared.Storage;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;

namespace Content.Server._White.Items.PseudoItem;

public sealed class PseudoItemSystem : SharedPseudoItemSystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StorageSystem _storageSystem = default!;
    [Dependency] private readonly ItemSystem _itemSystem = default!;
    [Dependency] private readonly CarryingSystem _carrying = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PseudoItemComponent, EntGotRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<PseudoItemComponent, PseudoItemInsertEvent>(OnInsert);
        SubscribeLocalEvent<PseudoItemComponent, CarryDoAfterEvent>(OnCarryEvent,
            after: new[] {typeof(CarryingSystem)});
        SubscribeLocalEvent<StorageComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb);
        SubscribeLocalEvent<StorageComponent, GetVerbsEvent<Verb>>(AddVerb);
        SubscribeLocalEvent<StorageComponent, PseudoItemInteractEvent>(OnInteract);
    }

    private void OnCarryEvent(Entity<PseudoItemComponent> ent, ref CarryDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;
        _transform.AttachToGridOrMap(ent);
    }

    private void OnInsert(Entity<PseudoItemComponent> ent, ref PseudoItemInsertEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (!TryComp(args.Target, out StorageComponent? storage))
            return;

        if (!TryInsert(args.Target.Value, ent.Owner, args.User, ent.Comp, storage))
            return;

        if (args.User != ent.Owner)
            _carrying.DropCarried(args.User, ent.Owner, false, false);
    }

    private void OnInteract(Entity<StorageComponent> ent, ref PseudoItemInteractEvent args)
    {
        if (!TryComp(args.Used, out PseudoItemComponent? pseudoItem))
            return;

        TryStartInsertDoAfter(ent.Owner, args.Used, args.User, pseudoItem, ent.Comp, args.VirtualItem);
    }

    private void AddAltVerb(Entity<StorageComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var user = args.User;

        var (uid, comp) = ent;

        if (!TryComp(user, out PseudoItemComponent? pseudoItem) || pseudoItem.Active)
            return;

        if (Transform(uid).ParentUid == user)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                TryStartInsertDoAfter(uid, user, user, pseudoItem, comp);
            },
            Text = Loc.GetString("action-name-insert-self"),
        };
        args.Verbs.Add(verb);
    }

    private void AddVerb(Entity<StorageComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var user = args.User;

        var (uid, comp) = ent;

        if (!TryComp(user, out CarryingComponent? carrying))
            return;

        if (!TryComp(user, out HandsComponent? hands) || !HasComp<VirtualItemComponent>(hands.ActiveHandEntity))
            return;

        var carried = carrying.Carried;

        if (!TryComp(carried, out PseudoItemComponent? pseudoItem))
            return;

        Verb verb = new()
        {
            Act = () =>
            {
                TryStartInsertDoAfter(uid, carried, user, pseudoItem, comp, hands.ActiveHandEntity.Value);
            },
            Text = Loc.GetString("action-name-insert-other"),
        };
        args.Verbs.Add(verb);
    }

    private void OnEntRemoved(EntityUid uid, PseudoItemComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (!component.Active)
            return;

        RemComp<CanEscapeInventoryComponent>(uid);
        RemComp<ItemComponent>(uid);
        component.Active = false;
        Dirty(uid, component);
    }

    private void TryStartInsertDoAfter(EntityUid storageUid,
        EntityUid toInsert,
        EntityUid user,
        PseudoItemComponent component,
        StorageComponent storage,
        EntityUid? used = null)
    {
        if (!FitsInContainer(storageUid, storage, component))
        {
            _popupSystem.PopupEntity(Loc.GetString("comp-storage-too-big"), user, user);
            return;
        }

        var args = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(1), new PseudoItemInsertEvent(),
            toInsert, storageUid, used)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = user != toInsert
        };

        if (!_doAfter.TryStartDoAfter(args))
            return;

        var message = toInsert == user
            ? Loc.GetString("action-start-insert-self", ("storage", storageUid))
            : Loc.GetString("action-start-insert-other",
                ("storage", storageUid), ("user", Identity.Entity(user, EntityManager)));

        _popupSystem.PopupEntity(message, toInsert, toInsert);
    }

    protected override void OnGettingPickedUp(Entity<PseudoItemComponent> ent, GettingPickedUpAttemptEvent args)
    {
        base.OnGettingPickedUp(ent, args);

        if (args.User == args.Item)
            return;

        if (!TryComp(ent, out CarriableComponent? carriable))
            _transform.AttachToGridOrMap(ent);
        else
            _carrying.StartCarryDoAfter(args.User, ent, carriable);
    }

    public bool TryInsert(EntityUid storageUid,
        EntityUid toInsert,
        EntityUid user,
        PseudoItemComponent component,
        StorageComponent storage)
    {
        if (TryComp(toInsert, out CanEscapeInventoryComponent? canEscape) && canEscape.DoAfter != null)
            _doAfter.Cancel(canEscape.DoAfter);

        var item = EnsureComp<ItemComponent>(toInsert);
        _itemSystem.SetSize(toInsert, component.Size, item);
        _itemSystem.SetShape(toInsert, component.Shape, item);

        Dirty(toInsert, component);
        if (!_storageSystem.Insert(storageUid, toInsert, out _, user, storage))
        {
            _popupSystem.PopupEntity(Loc.GetString("comp-storage-cant-insert"), user, user);
            component.Active = false;
            RemComp<ItemComponent>(toInsert);
            return false;
        }

        component.Active = true;
        EnsureComp<CanEscapeInventoryComponent>(toInsert).BaseResistTime = 3f;
        return true;
    }

    private bool FitsInContainer(EntityUid storageUid, StorageComponent storage, PseudoItemComponent pseudoItem)
    {
        return _storageSystem.GetMaxItemSize((storageUid, storage)) >= _itemSystem.GetSizePrototype(pseudoItem.Size);
    }
}
