using Content.Client.Overlays;
using Content.Shared._White.DeadWithoutMind;
using Content.Shared.Humanoid;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._White.DeadWithoutMind;

public sealed class ShowDeadWithoutMindSystem : EquipmentHudSystem<ShowDeadWithoutMindComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(Entity<HumanoidAppearanceComponent> entity, ref GetStatusIconsEvent args)
    {
        if (!IsActive || args.InContainer)
            return;

        if (!TryComp<MindContainerComponent>(entity.Owner, out var mindContainer))
            return;

        var dead = _mobStateSystem.IsDead(entity.Owner);

        if (!dead)
            return;

        if (mindContainer.Mind != null)
            return;

        if (_prototype.TryIndex<StatusIconPrototype>(entity.Comp.DeadWithoutMindIcon.Id, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
