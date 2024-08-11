using Content.Server.EUI;
using Content.Server.Popups;
using Content.Server._White.Cult.Runes.Comps;
using Content.Server._White.Cult.Runes.Systems;
using Content.Shared._White.Cult.Components;
using Content.Shared.Eui;
using Content.Shared.Popups;
using Content.Shared._White.Cult.UI;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._White.Cult.UI;

public sealed class CultBloodSpellsEui : BaseEui
{
    private readonly IEntityManager _entityManager;
    private readonly CultSystem _cult;

    private readonly EntityUid _performer;

    public CultBloodSpellsEui(EntityUid performer, IEntityManager entityManager)
    {
        _entityManager = entityManager;
        _cult = _entityManager.System<CultSystem>();
        _performer = performer;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not BloodSpellMessage cast || cast.State == BloodSpellMessageState.Cancel)
        {
            Close();
            return;
        }

        if (!_entityManager.TryGetComponent(_performer, out CultistComponent? cultist))
        {
            Close();
            return;
        }

        if (cast.State == BloodSpellMessageState.Create)
            _cult.CreateSpell((_performer, cultist), Player);
        else if (cast.State == BloodSpellMessageState.Remove)
            _cult.RemoveSpell((_performer, cultist), Player);

        Close();
    }
}
