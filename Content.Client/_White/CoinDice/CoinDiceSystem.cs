using Content.Shared._White.CoinDice;
using Robust.Client.GameObjects;

namespace Content.Client._White.CoinDice;

public sealed class CoinDiceSystem : SharedCoinDiceSystem
{
    protected override void UpdateVisuals(EntityUid uid, CoinDiceComponent? die = null)
    {
        if (!Resolve(uid, ref die) || !TryComp(uid, out SpriteComponent? sprite))
            return;

        var state = sprite.LayerGetState(0).Name;
        if (state == null)
            return;

        var prefix = state.Substring(0, state.IndexOf('_'));
        sprite.LayerSetState(0, $"{prefix}_{die.CurrentValue}");
    }

}
