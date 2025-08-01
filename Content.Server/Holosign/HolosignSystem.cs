using Content.Server._White.Holo;
using Content.Shared.Examine;
using Content.Server.Popups;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Holosign;

public sealed class HolosignSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HolosignProjectorComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
        SubscribeLocalEvent<HolosignProjectorComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<HolosignProjectorComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<HolosignProjectorComponent, UseInHandEvent>(OnUse);
    }

    private void OnExamine(EntityUid uid, HolosignProjectorComponent component, ExaminedEvent args)
    {
        var charges = UsesRemaining(component);
        var maxCharges = component.Uses;
        var activeholo = ActiveHolo(component);

        using (args.PushGroup(nameof(HolosignProjectorComponent)))
        {
            args.PushMarkup(Loc.GetString("limited-charges-charges-remaining", ("charges", charges)));
            args.PushMarkup(Loc.GetString("holoprojector-active-holo", ("activeholo", activeholo)));

            if (charges > 0 && charges == maxCharges)
            {
                args.PushMarkup(Loc.GetString("limited-charges-max-charges"));
            }
        }
    }

    private void OnUse(EntityUid uid, HolosignProjectorComponent comp, UseInHandEvent args)
    {
        foreach (var sign in comp.Signs.ShallowClone())
        {
            comp.Signs.Remove(sign);
            QueueDel(sign);
        }

        _popupSystem.PopupEntity(Loc.GetString("holoprojector-delete-signs"), args.User, args.User, PopupType.Medium);
    }

    private void OnBeforeInteract(EntityUid uid, HolosignProjectorComponent component, BeforeRangedInteractEvent args)
    {
        if (component.Signs.Contains(args.Target)) // wd edit
        {
            QueueDel(args.Target);
            component.Signs.Remove(args.Target);
            return;
        }

        if (args.Handled || !args.CanReach || args.Target != null)
            return;

        if (TryComp(uid, out UseDelayComponent? useDelay) && !_useDelay.TryResetDelay((uid, useDelay), true))
            return;

        if (component.Signs.Count >= component.Uses) // wd edit
        {
            _popupSystem.PopupEntity(Loc.GetString("holoprojector-uses-limit"), args.User, args.User, PopupType.Medium);
            return;
        }

        var holoUid = EntityManager.SpawnEntity(component.SignProto, args.ClickLocation.SnapToGrid(EntityManager));
        var xform = Transform(holoUid);
        if (!xform.Anchored)
        {
            _transform.AnchorEntity(holoUid, xform);
        }

        args.Handled = true;
        // WD EDIT
        EnsureComp<HoloComponent>(holoUid, out var holo);
        component.Signs.Add(holoUid);
        holo.Sign = uid;
        // WD EDIT
    }

    private void OnComponentRemove(EntityUid uid, HolosignProjectorComponent comp, ComponentRemove args) // wd edit
    {
        foreach (var sign in comp.Signs)
        {
            QueueDel(sign);
        }
    }

    private static int UsesRemaining(HolosignProjectorComponent component)
    {
        return component.Uses - component.Signs.Count; // wd edit
    }

    private static int ActiveHolo(HolosignProjectorComponent component) // wd edit
    {
        return component.Signs.Count; // wd edit
    }
}
