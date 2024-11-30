using Content.Shared._White.CoinDice;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server._White.CoinDice;

[UsedImplicitly]
public sealed class CoinDiceSystem : SharedCoinDiceSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Roll(EntityUid uid, CoinDiceComponent? die = null)
    {
        if (!Resolve(uid, ref die))
            return;

        var roll = _random.Next(1, die.Sides + 1);
        SetCurrentSide(uid, roll, die);
        var coindiceResult = "";
        switch (die.CurrentValue)
        {
            case 1:
                coindiceResult = "орёл";
                break;
            case 2:
                coindiceResult = "решка";
                break;
            default:
                coindiceResult = "ребро";
                break;
        }

        _popup.PopupEntity(Loc.GetString("coindice-component-on-roll-land", ("die", uid), ("currentSide", coindiceResult)), uid);
        _audio.PlayPvs(die.Sound, uid);
    }
}
