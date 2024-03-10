using Content.Shared._Amour.HumanoidAppearanceExtension;
using Content.Shared.Humanoid;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._Amour.CustomHeight;

public abstract class SharedCustomHeightSystem : EntitySystem
{
    [Dependency] protected readonly SharedAppearanceSystem AppearanceSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CustomHeightComponent,ComponentInit>(OnInit);
        SubscribeLocalEvent<CustomHeightComponent,HumanoidAppearanceLoadedEvent>(OnLoaded);
    }

    private void OnLoaded(EntityUid uid, CustomHeightComponent component, HumanoidAppearanceLoadedEvent args)
    {
        SetHeight(uid,GetHeightFromByte(uid,args.Profile.Appearance.Height));
    }

    private void OnInit(EntityUid uid, CustomHeightComponent component, ComponentInit args)
    {
        if(HasComp<HumanoidAppearanceComponent>(uid))
            return;

        if (component.Random)
            component.Starting = _robustRandom.NextFloat(component.Min, component.Max);

        if(component.Starting == 1f)
            return;

        SetHeight(uid,component.Starting);
    }

    public void SetHeight(Entity<CustomHeightComponent?> entity, float height)
    {
        if (!Resolve(entity, ref entity.Comp))
            entity.Comp = EnsureComp<CustomHeightComponent>(entity);

        height = Math.Clamp(height, entity.Comp.Min, entity.Comp.Max);

        AppearanceSystem.SetData(entity,HeightVisuals.State, height);
    }

    public float GetHeightFromByte(Entity<CustomHeightComponent?> entity, byte per)
    {
        if (!Resolve(entity, ref entity.Comp))
            entity.Comp = EnsureComp<CustomHeightComponent>(entity);

        var percent = (float)per / byte.MaxValue;
        var min = entity.Comp.Min;
        var max = entity.Comp.Max;

        return min + (max - min) * percent;
    }

    public byte GetByteFromHeight(Entity<CustomHeightComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            entity.Comp = EnsureComp<CustomHeightComponent>(entity);

        var min = entity.Comp.Min;
        var max = entity.Comp.Max;

        if (!AppearanceSystem.TryGetData<float>(entity, HeightVisuals.State, out var height))
            height = min;

        return (byte) ((height - min) / (max - min) * byte.MaxValue);
    }
}

[Serializable, NetSerializable]
public enum HeightVisuals : byte
{
    State
}
