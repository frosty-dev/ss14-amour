using JetBrains.Annotations;
using Robust.Shared.Player;

namespace Content.Shared._Amour.NeTrogat;

/// <summary>
///     System that handles <see cref="ActorComponent"/>.
/// </summary>
public sealed class AmourActorSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AmourActorComponent, ComponentShutdown>(OnActorShutdown);
    }

    private void OnActorShutdown(EntityUid entity, AmourActorComponent component, ComponentShutdown args)
    {
        _playerManager.SetAttachedEntity(component.PlayerSession, null);
    }

    [PublicAPI]
    public bool TryGetSession(EntityUid? uid, out ICommonSession? session)
    {
        if (TryComp(uid, out AmourActorComponent? actorComp))
        {
            session = actorComp.PlayerSession;
            return true;
        }

        session = null;
        return false;
    }

    [PublicAPI]
    [Pure]
    public ICommonSession? GetSession(EntityUid? uid)
    {
        if (TryComp(uid, out AmourActorComponent? actorComp))
        {
            return actorComp.PlayerSession;
        }

        return null;
    }
}
