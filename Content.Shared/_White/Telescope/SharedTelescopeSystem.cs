using System.Numerics;
using Content.Shared.Camera;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Item;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Telescope;

public abstract class SharedTelescopeSystem : EntitySystem
{
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<EyeOffsetChangedEvent>(OnEyeOffsetChanged);
        SubscribeLocalEvent<TelescopeComponent, GotUnequippedHandEvent>(OnUnequip);
        SubscribeLocalEvent<TelescopeComponent, HandDeselectedEvent>(OnHandDeselected);
        SubscribeLocalEvent<TelescopeComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<TelescopeComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp(ent.Comp.LastHoldingEntity, out EyeComponent? eye))
            return;

        SetOffset((ent.Comp.LastHoldingEntity.Value, eye), Vector2.Zero, ent);
    }

    private void OnHandDeselected(Entity<TelescopeComponent> ent, ref HandDeselectedEvent args)
    {
        if (!TryComp(args.User, out EyeComponent? eye))
            return;

        SetOffset((args.User, eye), Vector2.Zero, ent);
    }

    private void OnUnequip(Entity<TelescopeComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (!TryComp(args.User, out EyeComponent? eye))
            return;

        if (!HasComp<ItemComponent>(ent.Owner))
            return;

        SetOffset((args.User, eye), Vector2.Zero, ent);
    }

    public EntityUid GetRightEntity(EntityUid? ent)
    {
        var entity = EntityUid.Invalid;

        if (TryComp<HandsComponent>(ent, out var hands) &&
            HasComp<TelescopeComponent>(hands.ActiveHandEntity) &&
            hands.ActiveHandEntity.HasValue)
        {
            entity = hands.ActiveHandEntity.Value;
        }
        else if (HasComp<TelescopeComponent>(ent))
        {
            entity = ent.Value;
        }

        return entity;
    }

    private void OnEyeOffsetChanged(EyeOffsetChangedEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } ent)
            return;

        if (!TryComp(ent, out EyeComponent? eye))
            return;

        var entity = GetRightEntity(ent);

        if (entity == EntityUid.Invalid)
            return;

        var telescope = Comp<TelescopeComponent>(entity);

        var offset = Vector2.Lerp(eye.Offset, msg.Offset, telescope.LerpAmount);

        SetOffset((ent, eye), offset, telescope);
    }

    private void SetOffset(Entity<EyeComponent> ent, Vector2 offset, TelescopeComponent telescope)
    {
        telescope.LastHoldingEntity = ent;

        if (TryComp(ent, out CameraRecoilComponent? recoil))
        {
            recoil.BaseOffset = offset;
            _eye.SetOffset(ent, offset + recoil.CurrentKick, ent);
        }
        else
            _eye.SetOffset(ent, offset, ent);
    }

    public void SetParameters(Entity<TelescopeComponent> ent, float? divisor = null, float? lerpAmount = null)
    {
        var telescope = ent.Comp;

        divisor ??= telescope.Divisor;
        lerpAmount ??= telescope.LerpAmount;

        telescope.Divisor = divisor.Value;
        telescope.LerpAmount = lerpAmount.Value;

        Dirty(ent.Owner, telescope);
    }
}

[Serializable, NetSerializable]
public sealed class EyeOffsetChangedEvent : EntityEventArgs
{
    public Vector2 Offset;
}
