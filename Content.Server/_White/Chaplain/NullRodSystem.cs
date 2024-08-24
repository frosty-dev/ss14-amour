using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared._White.Chaplain;
using Content.Shared.Ghost;

namespace Content.Server._White.Chaplain;

public sealed class NullRodSystem : EntitySystem
{
    [Dependency] private readonly HandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NullRodComponent, WeaponSelectedEvent>(OnWeaponSelected);
    }

    private void OnWeaponSelected(Entity<NullRodComponent> ent, ref WeaponSelectedEvent args)
    {
        var entity = args.Actor;

        if (args.SelectedWeapon == string.Empty)
            return;

        var weapon = Spawn(args.SelectedWeapon, Transform(entity).Coordinates);

        Del(ent);

        _hands.PickupOrDrop(entity, weapon, true, false, false);
    }
}
