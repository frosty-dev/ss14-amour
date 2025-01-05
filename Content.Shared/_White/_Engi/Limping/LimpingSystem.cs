using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Robust.Shared.Timing;
using Robust.Shared.GameStates;
using Robust.Shared.Containers;

namespace Content.Shared._White._Engi.Limping;

public sealed class LimpingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LimpingHelperComponent, GotEquippedHandEvent>(OnGotEquipped);
        SubscribeLocalEvent<LimpingHelperComponent, GotUnequippedHandEvent>(OnGotUnequipped);

        SubscribeLocalEvent<LimpingComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<LimpingComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<LimpingComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<LimpingComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnShutdown(Entity<LimpingComponent> ent, ref ComponentShutdown args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefreshMoveSpeed(EntityUid uid, LimpingComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.SpeedModifier, component.SpeedModifier);
    }

    private void OnGotEquipped(Entity<LimpingHelperComponent> ent, ref GotEquippedHandEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        if (!TryComp<LimpingComponent>(args.User, out var comp))
            return;

        if (_gameTiming.IsFirstTimePredicted)
            comp.SpeedModifier = 0.5f;

        _movementSpeed.RefreshMovementSpeedModifiers(args.User);
    }

    private void OnGotUnequipped(Entity<LimpingHelperComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (!TryComp<LimpingComponent>(args.User, out var comp))
            return;

        if (_gameTiming.IsFirstTimePredicted)
            comp.SpeedModifier = 0.3f;

        _movementSpeed.RefreshMovementSpeedModifiers(args.User);
    }

    private void OnGetState(EntityUid uid, LimpingComponent component, ref ComponentGetState args)
    {
        args.State = new LimpingComponentState(component.SpeedModifier);
    }

    private void OnHandleState(EntityUid uid, LimpingComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not LimpingComponentState state)
            return;

        var diff = !MathHelper.CloseTo(component.SpeedModifier, state.SpeedModifier);

        if (diff && _container.TryGetContainingContainer(uid, out var container))
        {
            component.SpeedModifier = state.SpeedModifier;
            _movementSpeed.RefreshMovementSpeedModifiers(container.Owner);
        }
    }
}
