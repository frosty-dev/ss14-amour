using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Antag;

public sealed class AntagSelectionPlayerPool
{
    private readonly List<ICommonSession> _premiumPool;
    private readonly List<ICommonSession> _defaultPool;

    public AntagSelectionPlayerPool(List<List<ICommonSession>> orderedPools)
    {
        _premiumPool = orderedPools[0];
        _defaultPool = orderedPools[1];
    }

    public bool TryPickAndTake(IRobustRandom random, [NotNullWhen(true)] out ICommonSession? session)
    {
        session = null;

        if (_premiumPool.Count > 0)
        {
            session = random.PickAndTake(_premiumPool);
        }
        else if (_defaultPool.Count > 0)
        {
            session = random.PickAndTake(_defaultPool);
        }

        return session != null;
    }

    public int Count => _premiumPool.Count + _defaultPool.Count;
}
