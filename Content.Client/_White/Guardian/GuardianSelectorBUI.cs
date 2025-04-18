using Content.Client._White.UserInterface.Radial;
using Content.Shared._White.Guardian;

namespace Content.Client._White.Guardian;

public sealed class GuardianSelectorBUI : BoundUserInterface
{
    private readonly Dictionary<GuardianSelector, string> _names = new()
    {
        { GuardianSelector.Assasin, Loc.GetString("guardian-assasin-name")},
        { GuardianSelector.Charger, Loc.GetString("guardian-charger-name")},
        { GuardianSelector.Lighting, Loc.GetString("guardian-lighting-name")},
        { GuardianSelector.Standart, Loc.GetString("guardian-standart-name")},
    };

    private readonly Dictionary<GuardianSelector, string> _icons = new()
    {
        { GuardianSelector.Assasin, "/Textures/White/Interface/guardianselector.rsi/assasin.png" },
        { GuardianSelector.Charger, "/Textures/White/Interface/guardianselector.rsi/charger.png" },
        { GuardianSelector.Lighting, "/Textures/White/Interface/guardianselector.rsi/lighting.png" },
        { GuardianSelector.Standart, "/Textures/White/Interface/guardianselector.rsi/standart.png" },
    };

    private readonly Dictionary<string, GuardianSelector> _guardianSelectors = new()
    {
        { "Assasin", GuardianSelector.Assasin },
        { "Charger", GuardianSelector.Charger },
        { "Lighting", GuardianSelector.Lighting },
        { "Standart", GuardianSelector.Standart },
    };

    private RadialContainer? _radialContainer;
    private bool _updated;

    public GuardianSelectorBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        if (_radialContainer != null)
            UIReset();

        _radialContainer = new RadialContainer();

        _radialContainer.Closed += Close;

        if (State != null)
            UpdateState(State);
    }

    private void UIReset()
    {
        _radialContainer?.Close();
        _radialContainer = null;
        _updated = false;
    }

    private void PopulateRadial(IReadOnlyCollection<string> ids, NetEntity target)
    {
        foreach (var id in ids)
        {
            if (_radialContainer == null)
                continue;

            if(!_guardianSelectors.TryGetValue(id, out var guardianSelector))
                return;

            if(!_names.TryGetValue(guardianSelector, out var name) || !_icons.TryGetValue(guardianSelector, out var icon))
                return;

            var button = _radialContainer.AddButton(name, icon);
            button.Controller.OnPressed += _ =>
            {
                Select(guardianSelector, target);
            };
        }
    }

    private void Select(GuardianSelector type, NetEntity target)
    {
        SendMessage(new GuardianSelectorSelectedBuiMessage(type, target));
        UIReset();
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _radialContainer?.Close();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_updated)
            return;

        if (state is GuardianSelectorBUIState newState)
        {
            PopulateRadial(newState.Ids, newState.Target);
        }

        if (_radialContainer == null)
            return;

        _radialContainer?.OpenAttachedLocalPlayer();
        _updated = true;
    }
}

