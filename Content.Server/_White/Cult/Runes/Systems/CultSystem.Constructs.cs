using System.Linq;
using Content.Shared._White.Blocking;
using Content.Shared._White.Cult.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Mind.Components;
using Content.Shared._White.Cult.UI;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using ConstructShellComponent = Content.Shared._White.Cult.Components.ConstructShellComponent;

namespace Content.Server._White.Cult.Runes.Systems;

public partial class CultSystem
{
    [Dependency] private readonly ItemSlotsSystem _slotsSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public void InitializeConstructs()
    {
        SubscribeLocalEvent<ConstructShellComponent, ContainerIsInsertingAttemptEvent>(OnShardInsertAttempt);
        SubscribeLocalEvent<ConstructShellComponent, ComponentInit>(OnShellInit);
        SubscribeLocalEvent<ConstructShellComponent, ComponentRemove>(OnShellRemove);
        SubscribeLocalEvent<ConstructShellComponent, ConstructFormSelectedEvent>(OnShellSelected);
        SubscribeLocalEvent<ConstructComponent, MeleeHitEvent>(OnMeleeHit, before: new []{typeof(MeleeBlockSystem)});
    }

    private void OnMeleeHit(Entity<ConstructComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Any(e => HasComp<CultistComponent>(e) || HasComp<ConstructComponent>(e)))
            args.BonusDamage = -args.BaseDamage;
    }

    private void OnShellSelected(EntityUid uid, ConstructShellComponent component, ConstructFormSelectedEvent args)
    {
        var construct = Spawn(args.SelectedForm, _transform.GetMapCoordinates(args.Actor));
        var mind = Comp<MindContainerComponent>(args.Actor);

        if (!mind.HasMind)
            return;

        _mindSystem.TransferTo(mind.Mind.Value, construct);
        Del(uid);
    }

    private void OnShellInit(EntityUid uid, ConstructShellComponent component, ComponentInit args)
    {
        _slotsSystem.AddItemSlot(uid, component.ShardSlotId, component.ShardSlot);
    }

    private void OnShellRemove(EntityUid uid, ConstructShellComponent component, ComponentRemove args)
    {
        _slotsSystem.RemoveItemSlot(uid, component.ShardSlot);
    }

    private void OnShardInsertAttempt(EntityUid uid, ConstructShellComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (!TryComp<MindContainerComponent>(args.EntityUid, out var mindContainer) || !mindContainer.HasMind || !HasComp<ActorComponent>(args.EntityUid))
        {
            _popupSystem.PopupEntity("Нет души", uid);
            args.Cancel();
            return;
        }

        _slotsSystem.SetLock(uid, component.ShardSlotId, true);
        _ui.OpenUi(uid, SelectConstructUi.Key, args.EntityUid);
    }
}
