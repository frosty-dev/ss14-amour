using System.Text.RegularExpressions;
using Content.Shared._White.Economy;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Timing;

namespace Content.Client._White.Economy.Ui;

[GenerateTypedNameReferences]
public sealed partial class AtmWindow : DefaultWindow
{
    public Action<ATMRequestWithdrawMessage>? OnWithdrawAttempt;

    private readonly string _pinPattern = "[^0-9]";
    public AtmWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);


        WithdrawButton.OnButtonDown += args =>
        {
            if(PinLineEdit.Text.Length != 4) return;
            OnWithdrawAttempt?.Invoke(new ATMRequestWithdrawMessage(WithdrawSlider.Value, int.Parse((string) PinLineEdit.Text)));
        };

        PinLineEdit.OnTextChanged += _ =>
        {
            ValidatePin();
        };
    }

    public void UpdateState(BoundUserInterfaceState state)
    {
        if(state is not ATMBuiState cast) return;

        if (!cast.HasCard)
        {
            StatusLabel.Text = cast.InfoMessage;
            BalanceLabel.Visible = false;
            Divider.Visible = false;
            StatusLabel.Visible = true;
            WithdrawSlider.Visible = false;
            PinLineEdit.Visible = false;
            WithdrawButton.Visible = false;
            return;
        }

        StatusLabel.Text = cast.InfoMessage;

        BalanceLabel.Text = Loc.GetString("atm-ui-balance", ("balance", cast.AccountBalance));
        BalanceLabel.Visible = true;

        if (cast.AccountBalance > 0)
        {
            Divider.Visible = true;
            StatusLabel.Visible = true;
            WithdrawSlider.Visible = true;
            PinLineEdit.Visible = true;
            WithdrawButton.Visible = true;

            WithdrawSlider.MaxValue = cast.AccountBalance;
            WithdrawSlider.Value = Math.Min((int) WithdrawSlider.Value, cast.AccountBalance);
            return;
        }

        Divider.Visible = false;
        StatusLabel.Visible = false;
        WithdrawSlider.Visible = false;
        PinLineEdit.Visible = false;
        WithdrawButton.Visible = false;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        WithdrawButton.Disabled = PinLineEdit.Text.Length != 4;
    }

    private void ValidatePin()
    {
        var pinText = Regex.Replace(PinLineEdit.Text, _pinPattern, string.Empty);

        if (pinText.Length > 4)
        {
            pinText = pinText[..4];
        }

        PinLineEdit.Text = pinText;
    }
}
