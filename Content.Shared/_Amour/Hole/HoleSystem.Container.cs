using Content.Shared._Amour.Arousal;
using Content.Shared._Amour.HumanoidAppearanceExtension;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Amour.Hole;

public abstract partial class SharedHoleSystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    public void InitializeContainer()
    {
        SubscribeLocalEvent<HoleContainerComponent, ComponentInit>(OnContainerInit);
        SubscribeLocalEvent<HoleContainerComponent, HumanoidAppearanceLoadedEvent>(OnAppearanceLoaded);
        SubscribeLocalEvent<HoleContainerComponent, HumanoidAppearanceClonedEvent>(OnClone);
    }

    private void OnClone(EntityUid uid, HoleContainerComponent component, HumanoidAppearanceClonedEvent args)
    {
        var holeContainerComponent = EnsureComp<HoleContainerComponent>(args.Target);
        foreach (var entity in component.Slot.ContainedEntities)
        {
            var meta = MetaData(entity);
            if (meta.EntityPrototype is null || !TryComp<HoleComponent>(entity, out var holeComponent))
                continue;
            AddHole(new Entity<HoleContainerComponent?>(args.Target, holeContainerComponent), meta.EntityPrototype.ID, holeComponent.Layers[0].Color);
        }

        Dirty(args.Target, holeContainerComponent);
    }

    private void OnAppearanceLoaded(EntityUid uid, HoleContainerComponent component, HumanoidAppearanceLoadedEvent args)
    {
        foreach (var genitals in args.Profile.Appearance.Genitals)
        {
            AddHole(new Entity<HoleContainerComponent?>(uid, component), genitals.GenitalId, genitals.Color);
        }
    }

    private void OnContainerInit(EntityUid uid, HoleContainerComponent component, ComponentInit args)
    {
        component.Slot = _containerSystem.EnsureContainer<Container>(uid, HoleContainerComponent.SlotName);
        foreach (var protoId in component.HolePrototypes)
        {
            AddHole(new Entity<HoleContainerComponent?>(uid, component), protoId);
        }
    }

    public void AddHole(Entity<HoleContainerComponent?> entity, EntProtoId protoId, Color? color = null)
    {
        if (!_prototypeManager.TryIndex<EntityPrototype>(protoId, out _))
        {
            Log.Error(protoId + " NOT EXIST YOU BASTARD!");
            return;
        }
        if (!Resolve(entity.Owner, ref entity.Comp, logMissing: false))
            entity.Comp = EnsureComp<HoleContainerComponent>(entity.Owner);

        var spawned = Spawn(protoId);
        if (!TryComp<HoleComponent>(spawned, out var component))
        {
            QueueDel(spawned);
            return;
        }

        component.Layers[0].Color = color;

        if (component.IsMainHole)
            entity.Comp.MainHole = GetNetEntity(spawned);

        _containerSystem.Insert(spawned, entity.Comp.Slot);
        Dirty(spawned, component);
    }
}
