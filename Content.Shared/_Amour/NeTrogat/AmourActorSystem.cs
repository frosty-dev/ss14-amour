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
        SubscribeLocalEvent<ActorComponent, ComponentShutdown>(OnActorShutdown);
    }

    private void OnActorShutdown(EntityUid entity, ActorComponent component, ComponentShutdown args)
    {
        _playerManager.SetAttachedEntity(component.PlayerSession, null);
    }

    [PublicAPI]
    public bool TryGetSession(EntityUid? uid, out ICommonSession? session)
    {
        if (TryComp(uid, out ActorComponent? actorComp))
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
        if (TryComp(uid, out ActorComponent? actorComp))
        {
            return actorComp.PlayerSession;
        }

        return null;
    }
}
