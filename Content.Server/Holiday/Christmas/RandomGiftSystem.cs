﻿using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;
using Content.Server.Administration.Logs;
using Content.Server.Hands.Systems;
using Content.Server._White.Other;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Holiday.Christmas;

/// <summary>
/// This handles granting players their gift.
/// </summary>
public sealed class RandomGiftSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    private readonly List<string> _possibleGiftsSafe = new();
    private readonly List<string> _possibleGiftsUnsafe = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        SubscribeLocalEvent<RandomGiftComponent, MapInitEvent>(OnGiftMapInit);
        SubscribeLocalEvent<RandomGiftComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<RandomGiftComponent, ExaminedEvent>(OnExamined);
        BuildIndex();
    }

    private void OnExamined(EntityUid uid, RandomGiftComponent component, ExaminedEvent args)
    {
        if (!component.ContentsViewers.IsValid(args.Examiner, EntityManager) || component.SelectedEntity is null)
            return;

        var name = _prototype.Index<EntityPrototype>(component.SelectedEntity).Name;
        args.PushText(Loc.GetString("gift-packin-contains", ("name", name)));
    }

    private void OnUseInHand(EntityUid uid, RandomGiftComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (component.SelectedEntity is null)
            return;

        var coords = Transform(args.User).Coordinates;
        var handsEnt = Spawn(component.SelectedEntity, coords);
        _adminLogger.Add(LogType.EntitySpawn, LogImpact.Low, $"{ToPrettyString(args.User)} used {ToPrettyString(uid)} which spawned {ToPrettyString(handsEnt)}");
        EnsureComp<ItemComponent>(handsEnt); // For insane mode.
        if (component.Wrapper is not null)
            Spawn(component.Wrapper, coords);

        args.Handled = true;
        _audio.PlayPvs(component.Sound, args.User);
        Del(uid);
        _hands.PickupOrDrop(args.User, handsEnt);

    }

    private void OnGiftMapInit(EntityUid uid, RandomGiftComponent component, MapInitEvent args)
    {
        if (component.InsaneMode)
            component.SelectedEntity = _random.Pick(_possibleGiftsUnsafe);
        else
            component.SelectedEntity = _random.Pick(_possibleGiftsSafe);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs obj)
    {
        if (obj.WasModified<EntityPrototype>())
            BuildIndex();
    }

    private void BuildIndex()
    {
        _possibleGiftsSafe.Clear();
        _possibleGiftsUnsafe.Clear();
        var itemCompName = _componentFactory.GetComponentName(typeof(ItemComponent));
        var mapGridCompName = _componentFactory.GetComponentName(typeof(MapGridComponent));
        var physicsCompName = _componentFactory.GetComponentName(typeof(PhysicsComponent));
        var giftIgnoreCompName = _componentFactory.GetComponentName(typeof(GiftIgnoreComponent)); // WD
        var unremovableCompName = _componentFactory.GetComponentName(typeof(UnremoveableComponent)); // WD

        foreach (var proto in _prototype.EnumeratePrototypes<EntityPrototype>())
        {
            if (proto.Abstract || proto.NoSpawn || proto.Components.ContainsKey(mapGridCompName) || !proto.Components.ContainsKey(physicsCompName))
                continue;

            _possibleGiftsUnsafe.Add(proto.ID);

            if (!proto.Components.ContainsKey(itemCompName) || proto.Components.ContainsKey(giftIgnoreCompName) ||
                proto.Components.ContainsKey(unremovableCompName) || proto.SetSuffix != null &&
                new[] {"Debug, Admeme, Admin"}.Any(x =>
                    proto.SetSuffix.Contains(x, StringComparison.OrdinalIgnoreCase))) // WD EDIT
                continue;

            _possibleGiftsSafe.Add(proto.ID);
        }
    }

    // WD START
    public string? PickRandomItem()
    {
        return _possibleGiftsSafe.Count == 0 ? null : _random.Pick(_possibleGiftsSafe);
    }
    // WD END
}
