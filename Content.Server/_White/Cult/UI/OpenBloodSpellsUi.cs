using Content.Server.EUI;
using Content.Shared.Alert;
using JetBrains.Annotations;
using Robust.Shared.Player;

namespace Content.Server._White.Cult.UI;

[UsedImplicitly, DataDefinition]
public sealed partial class OpenBloodSpellsUi : IAlertClick
{
    public void AlertClicked(EntityUid player)
    {
        var euiManager = IoCManager.Resolve<EuiManager>();
        var entManager = IoCManager.Resolve<IEntityManager>();

        if (!entManager.TryGetComponent(player, out ActorComponent? actor))
            return;

        euiManager.OpenEui(new CultBloodSpellsEui(player, entManager), actor.PlayerSession);
    }
}
