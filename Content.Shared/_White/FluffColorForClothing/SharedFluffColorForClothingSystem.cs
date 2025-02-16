using Content.Shared.Actions;
using Content.Shared.Inventory.Events;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared._White.FluffColorForClothing;

public abstract class SharedFluffColorForClothingSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FluffColorForClothingComponent, GetVerbsEvent<AlternativeVerb>>(OnAddVerb);
        SubscribeLocalEvent<FluffColorForClothingComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        SubscribeLocalEvent<FluffColorForClothingComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FluffColorForClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<FluffColorForClothingComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<FluffColorForClothingComponent, FluffColorForClothingEvent>(OnEvent);
        SubscribeLocalEvent<FluffColorForClothingComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FluffColorForClothingComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<FluffColorForClothingComponent, ComponentRemove>(OnRemove);
    }

    private void OnRemove(Entity<FluffColorForClothingComponent> ent, ref ComponentRemove args)
    {
        _actionsSystem.RemoveAction(ent.Comp.ActionEntity);
    }

    private void OnGetActions(Entity<FluffColorForClothingComponent> ent, ref GetItemActionsEvent args)
    {
        if (ent.Comp.ActionEntity != null && args.SlotFlags == ent.Comp.RequiredFlags)
            args.AddAction(ent.Comp.ActionEntity.Value);
    }

    private void OnMapInit(Entity<FluffColorForClothingComponent> ent, ref MapInitEvent args)
    {
        if (_actionContainer.EnsureAction(ent.Owner, ref ent.Comp.ActionEntity, out var action, ent.Comp.Action))
            _actionsSystem.SetEntityIcon(ent.Comp.ActionEntity.Value, ent.Owner, action);
    }

    private void OnEvent(Entity<FluffColorForClothingComponent> ent, ref FluffColorForClothingEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ChangeColor(ent.Owner, ent.Comp);
    }

    private void OnUnequipped(EntityUid uid, FluffColorForClothingComponent component, GotUnequippedEvent args)
    {
        component.User = null;
    }

    private void OnEquipped(EntityUid uid, FluffColorForClothingComponent component, GotEquippedEvent args)
    {
        component.User = args.Equipee;
    }

    private void OnAfterHandleState(EntityUid uid, FluffColorForClothingComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(uid, component);
    }

    private void OnInit(EntityUid uid, FluffColorForClothingComponent component, ComponentInit args)
    {
        UpdateVisuals(uid, component);
    }

    private void OnAddVerb(EntityUid uid, FluffColorForClothingComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || component.Colors.Count < 2 || !component.MainItem)
            return;

        AlternativeVerb verb = new()
        {
            EventTarget = uid,
            ExecutionEventArgs = new FluffColorForClothingEvent() { Performer = args.User },
            Text = component.VerbText,
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/rotate_cw.svg.192dpi.png")),
            Priority = 0,
        };

        args.Verbs.Add(verb);
    }

    protected virtual void UpdateVisuals(EntityUid uid, FluffColorForClothingComponent component)
    {
        // See client system
    }

    protected virtual void ChangeColor(EntityUid uid, FluffColorForClothingComponent component)
    {
        // See server system
    }
}

public sealed partial class FluffColorForClothingEvent : InstantActionEvent
{
}
