using Content.Shared.Damage.Events;
using Content.Shared.Examine;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Stunnable;

namespace Content.Shared._White.Item.Telebaton;

public sealed class TelebatonSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedItemToggleSystem _itemToggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelebatonComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<TelebatonComponent, StaminaDamageOnHitAttemptEvent>(OnStaminaHitAttempt);
        SubscribeLocalEvent<TelebatonComponent, ItemToggledEvent>(ToggleDone);
        SubscribeLocalEvent<TelebatonComponent, StaminaMeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<TelebatonComponent> ent, ref StaminaMeleeHitEvent args)
    {
        var time = ent.Comp.KnockdownTime;
        if (time <= TimeSpan.Zero)
            return;

        foreach (var (uid, _) in args.HitList)
        {
            _stun.TryKnockdown(uid, time, true);
        }
    }

    private void OnStaminaHitAttempt(Entity<TelebatonComponent> entity, ref StaminaDamageOnHitAttemptEvent args)
    {
        if (!_itemToggle.IsActivated(entity.Owner))
            args.Cancelled = true;
    }

    private void OnExamined(Entity<TelebatonComponent> entity, ref ExaminedEvent args)
    {
        var onMsg = _itemToggle.IsActivated(entity.Owner)
            ? Loc.GetString("comp-telebaton-examined-on")
            : Loc.GetString("comp-telebaton-examined-off");
        args.PushMarkup(onMsg);
    }

    private void ToggleDone(Entity<TelebatonComponent> entity, ref ItemToggledEvent args)
    {
        _item.SetHeldPrefix(entity.Owner, args.Activated ? "on" : "off");
    }
}
