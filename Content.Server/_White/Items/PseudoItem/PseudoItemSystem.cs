using Content.Server._White.Carrying;
using Content.Shared.Verbs;
using Content.Shared.Item;
using Content.Shared.Hands;
using Content.Server.Storage.EntitySystems;
using Content.Server.Item;
using Content.Server.Popups;
using Content.Server.Resist;
using Content.Shared._White.Item.PseudoItem;
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

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PseudoItemComponent, EntGotRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<PseudoItemComponent, DropAttemptEvent>(OnDropAttempt);
        SubscribeLocalEvent<PseudoItemComponent, EscapeInventoryEvent>(OnEscape);
        SubscribeLocalEvent<StorageComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb);
        SubscribeLocalEvent<StorageComponent, GetVerbsEvent<Verb>>(AddVerb);
        SubscribeLocalEvent<StorageComponent, PseudoItemInteractEvent>(OnInteract);
    }

    private void OnInteract(Entity<StorageComponent> ent, ref PseudoItemInteractEvent args)
    {
        if (!TryComp(args.Used, out PseudoItemComponent? pseudoItem))
            return;

        if (!TryInsert(ent.Owner, args.Used, args.User, pseudoItem, ent.Comp))
            return;

        _carrying.DropCarried(args.User, args.Used, false, false);
    }

    private void OnEscape(Entity<PseudoItemComponent> ent, ref EscapeInventoryEvent args)
    {
        NoLongerInContainer(ent.Owner, ent.Comp);
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
                TryInsert(uid, user, user, pseudoItem, comp);
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

        var carried = carrying.Carried;

        if (!TryComp(carried, out PseudoItemComponent? pseudoItem))
            return;

        Verb verb = new()
        {
            Act = () =>
            {
                if (TryInsert(uid, carried, user, pseudoItem, comp))
                    _carrying.DropCarried(user, carried, false, false);
            },
            Text = Loc.GetString("action-name-insert-other"),
        };
        args.Verbs.Add(verb);
    }

    private void OnEntRemoved(EntityUid uid, PseudoItemComponent component, EntGotRemovedFromContainerMessage args)
    {
        NoLongerInContainer(uid, component);
    }

    protected override void OnGettingPickedUp(Entity<PseudoItemComponent> ent, GettingPickedUpAttemptEvent args)
    {
        base.OnGettingPickedUp(ent, args);

        if (args.User == args.Item)
            return;

        if (!TryComp(ent, out CarriableComponent? carriable))
            _transform.AttachToGridOrMap(ent);
        else if (_carrying.CanCarry(args.User, ent))
            _carrying.StartCarryDoAfter(args.User, ent, carriable);
    }

    private void OnDropAttempt(EntityUid uid, PseudoItemComponent component, DropAttemptEvent args)
    {
        if (component.Active)
            args.Cancel();
    }

    public bool TryInsert(EntityUid storageUid,
        EntityUid toInsert,
        EntityUid user,
        PseudoItemComponent component,
        StorageComponent? storage = null)
    {
        if (!Resolve(storageUid, ref storage))
            return false;

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
        EnsureComp<CanEscapeInventoryComponent>(toInsert);
        return true;
    }

    private void NoLongerInContainer(EntityUid uid, PseudoItemComponent component)
    {
        if (!component.Active)
            return;

        RemCompDeferred<CanEscapeInventoryComponent>(uid);
        RemCompDeferred<ItemComponent>(uid);
        component.Active = false;
        Dirty(uid, component);
    }
}
