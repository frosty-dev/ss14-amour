using Content.Client.Eui;
using Content.Shared._White.Cult.UI;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client._White.Cult.UI.CultBloodSpellsEui;

[UsedImplicitly]
public sealed class CultBloodSpellsEui : BaseEui
{
    private readonly CultBloodSpellsMenu _menu;
    private bool _messageSent;

    public CultBloodSpellsEui()
    {
        _menu = new CultBloodSpellsMenu();

        _menu.OnClose += () =>
        {
            if (_messageSent)
                return;
            SendMessage(new BloodSpellMessage(BloodSpellMessageState.Cancel));
            _messageSent = true;
        };

        _menu.RemoveButton.OnPressed += _ =>
        {
            Send(BloodSpellMessageState.Remove);
        };

        _menu.CreateButton.OnPressed += _ =>
        {
            Send(BloodSpellMessageState.Create);
        };
    }

    private void Send(BloodSpellMessageState state)
    {
        SendMessage(new BloodSpellMessage(state));
        _messageSent = true;
        _menu.Close();
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        _menu.Close();
    }

}
