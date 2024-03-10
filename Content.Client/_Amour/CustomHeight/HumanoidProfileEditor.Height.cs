using Content.Client._Amour.CustomHeight;
using Content.Shared._Amour.CustomHeight;
using Robust.Client.UserInterface.Controls;
using Range = Robust.Client.UserInterface.Controls.Range;

namespace Content.Client.Preferences.UI;
public sealed partial class HumanoidProfileEditor
{
    private Slider _height => CHeight;
    private Label _heightInformation => CHeightInformation;

    private CustomHeightSystem _customHeightSystem = default!;

    public void InitializeHeight()
    {
        _customHeightSystem = _entMan.System<CustomHeightSystem>();

        _height.OnValueChanged += HeightValueChanged;
        ResetHeightButton.OnPressed += ResetHeightButtonOnOnPressed;
    }

    private void ResetHeightButtonOnOnPressed(BaseButton.ButtonEventArgs obj)
    {
        if (_entMan.TryGetComponent<CustomHeightComponent>(_previewDummy, out var heightComponent))
        {
            SetDummyHeight(128);
        }
    }

    private void HeightValueChanged(Range obj)
    {
        SetDummyHeight((byte) _height.Value,false);
    }

    public void UpdateHeightControl()
    {
        if (Profile is null)
            return;

        if (!_entMan.TryGetComponent<CustomHeightComponent>(_previewDummy, out _))
        {
            HeightContainer.Visible = false;
            return;
        }

        HeightContainer.Visible = true;
        _height.Value = Profile.Appearance.Height;

        UpdateHeightText();
    }

    public void SetDummyHeight(byte height, bool changeHeightValue = true)
    {
        if (Profile is null || !_entMan.TryGetComponent<CustomHeightComponent>(_previewDummy, out _))
            return;

        if(changeHeightValue)
            _height.Value = height;

        Profile = Profile.WithCharacterAppearance(Profile.Appearance.WithHeight(height));

        UpdateHeightText();

        IsDirty = true;
    }

    public void UpdateHeightText()
    {
        if (_entMan.TryGetComponent<CustomHeightComponent>(_previewDummy, out _))
        {
            var height = (int)(_customHeightSystem
                .GetHeightFromByte(_previewDummy.Value, (byte) _height.Value) * 180);

            _heightInformation.Text = Loc.GetString("humanoid-profile-height-current") + height;
        }
    }
}
