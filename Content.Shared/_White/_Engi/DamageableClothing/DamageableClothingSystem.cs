using Robust.Shared.Timing;
using Content.Shared.Inventory.Events;

namespace Content.Shared._White._Engi.DamageableClothing;

/// <summary>
/// WD
/// </summary>
public sealed partial class DamageableClothingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeUser();

        SubscribeLocalEvent<DamageableClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<DamageableClothingComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<DamageableClothingComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnEquipped(EntityUid uid, DamageableClothingComponent component, GotEquippedEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;
        component.User = args.Equipee;
        var userComp = EnsureComp<DamageableClothingUserComponent>(args.Equipee);
        userComp.ItemId = args.Equipment;
    }

    private void OnUnequipped(EntityUid uid, DamageableClothingComponent component, GotUnequippedEvent args)
    {
        RemCompDeferred<DamageableClothingUserComponent>(args.Equipee);
        component.User = null;
    }

    private void OnShutdown(EntityUid uid, DamageableClothingComponent component, ComponentShutdown args)
    {
        if (component.User != null)
        {
            RemCompDeferred<DamageableClothingUserComponent>(component.User.Value);
            component.User = null;
        }
    }
}
