using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Throwing;
using Robust.Shared.Timing;

namespace Content.Shared._White.CoinDice;

public abstract class SharedCoinDiceSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CoinDiceComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<CoinDiceComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<CoinDiceComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CoinDiceComponent, AfterAutoHandleStateEvent>(OnDiceAfterHandleState);
    }

    private void OnDiceAfterHandleState(EntityUid uid, CoinDiceComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(uid, component);
    }

    private void OnUseInHand(EntityUid uid, CoinDiceComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Roll(uid, component);
    }

    private void OnLand(EntityUid uid, CoinDiceComponent component, ref LandEvent args)
    {
        Roll(uid, component);
    }

    private void OnExamined(EntityUid uid, CoinDiceComponent dice, ExaminedEvent args)
    {
        //No details check, since the sprite updates to show the side.
        using (args.PushGroup(nameof(CoinDiceComponent)))
        {
            args.PushMarkup(Loc.GetString("coindice-component-on-examine-message-part-1"));
            var coindiceResult = "";
            switch (dice.CurrentValue)
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
            args.PushMarkup(Loc.GetString("coindice-component-on-examine-message-part-2",
                ("currentSide", coindiceResult)));
        }
    }

    public void SetCurrentSide(EntityUid uid, int side, CoinDiceComponent? die = null)
    {
        if (!Resolve(uid, ref die))
            return;

        if (side < 1 || side > die.Sides)
        {
            Log.Error($"Attempted to set die {ToPrettyString(uid)} to an invalid side ({side}).");
            return;
        }

        die.CurrentValue = (side - die.Offset) * die.Multiplier;
        Dirty(uid, die);
        UpdateVisuals(uid, die);
    }

    public void SetCurrentValue(EntityUid uid, int value, CoinDiceComponent? die = null)
    {
        if (!Resolve(uid, ref die))
            return;

        if (value % die.Multiplier != 0 || value / die.Multiplier + die.Offset < 1)
        {
            Log.Error($"Attempted to set die {ToPrettyString(uid)} to an invalid value ({value}).");
            return;
        }

        SetCurrentSide(uid, value / die.Multiplier + die.Offset, die);
    }

    protected virtual void UpdateVisuals(EntityUid uid, CoinDiceComponent? die = null)
    {
        // See client system.
    }

    public virtual void Roll(EntityUid uid, CoinDiceComponent? die = null)
    {
        // See the server system, client cannot predict rolling.
    }

}
