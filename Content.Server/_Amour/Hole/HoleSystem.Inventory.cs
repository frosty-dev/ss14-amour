using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Shared._Amour.Hole;
using Content.Shared.Damage;
using Content.Shared.Item;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._Amour.Hole;

public sealed partial class HoleSystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;

    private void InitializeInventory()
    {
        SubscribeLocalEvent<HoleInventoryComponent,ComponentInit>(OnInventoryInit);
    }

    private void OnInventoryInit(EntityUid uid, HoleInventoryComponent component, ComponentInit args)
    {
        component.Slot = _container.EnsureContainer<ContainerSlot>(uid,HoleInventoryComponent.SlotName);
    }

    public void PutItem(Entity<ItemComponent?> item, Entity<HoleInventoryComponent?> entity)
    {
        if(!Resolve(entity,ref entity.Comp) || !Resolve(item,ref item.Comp))
            return;
        if(!_prototypeManager.TryIndex(item.Comp.Size, out var itemSizePrototype)
           || !_prototypeManager.TryIndex(entity.Comp.Size, out var holeSize)
           || !_prototypeManager.TryIndex(entity.Comp.MaxSize, out var maxHoleSize))
            return;

        if (itemSizePrototype.Weight >= maxHoleSize.Weight)
        {
            _bloodstreamSystem.TryModifyBleedAmount(entity, (itemSizePrototype.Weight - maxHoleSize.Weight)*8);
            _chatSystem.TrySendInGameICMessage(item.Owner," с грохотом падает на пол", InGameICChatType.Emote, ChatTransmitRange.HideChat);
            //_bodySystem.GibBody(entity);
            return;
        }

        if (itemSizePrototype.Weight >= holeSize.Weight)
        {
            _bloodstreamSystem.TryModifyBleedAmount(entity, (itemSizePrototype.Weight - holeSize.Weight)*4);
        }

        _container.Insert(item.Owner, entity.Comp.Slot);
    }

    public List<EntityUid> TakeItem(Entity<HoleInventoryComponent?> entity)
    {
        if(!Resolve(entity,ref entity.Comp))
            return new List<EntityUid>();

        return _container.EmptyContainer(entity.Comp.Slot);
    }
}
