using System.Linq;
using Content.Client._White.UserInterface.Radial;
using Content.Shared._White.SecurityHud;
using Content.Shared.Security;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Client._White.SecurityHud;

public sealed class SecurityHudBUI : BoundUserInterface
{
    private RadialContainer? _radialContainer;

    private bool _updated;

    private readonly Dictionary<string, string> _names = new()
    {
        { "SecurityIconDemote", Loc.GetString("criminal-records-status-demote")}, // WD start
        { "SecurityIconParoled", Loc.GetString("criminal-records-status-paroled")},
        { "SecurityIconSuspected", Loc.GetString("criminal-records-status-suspected")},
        { "SecurityIconWanted", Loc.GetString("criminal-records-status-wanted")},
        { "SecurityIconIncarcerated", Loc.GetString("criminal-records-status-detained")},
        { "SecurityIconExecute", Loc.GetString("criminal-records-status-execute")},
        { "SecurityIconMonitoring", Loc.GetString("criminal-records-status-monitoring")},
        { "SecurityIconReleased", Loc.GetString("criminal-records-status-released")},
        { "SecurityIconSearch", Loc.GetString("criminal-records-status-search")},
        { "CriminalRecordIconRemove", Loc.GetString("security-hud-remove-status") }
    };

    private readonly Dictionary<string, string> _icons = new()
    {
        { "SecurityIconDemote", "/Textures/White/Interface/securityhud.rsi/demote.png" },
        { "SecurityIconParoled", "/Textures/White/Interface/securityhud.rsi/paroled.png" },
        { "SecurityIconSuspected", "/Textures/White/Interface/securityhud.rsi/suspected.png" },
        { "SecurityIconWanted", "/Textures/White/Interface/securityhud.rsi/wanted.png" },
        { "SecurityIconIncarcerated", "/Textures/White/Interface/securityhud.rsi/incarcerated.png" },
        { "SecurityIconExecute", "/Textures/White/Interface/securityhud.rsi/execute.png" },
        { "SecurityIconMonitoring", "/Textures/White/Interface/securityhud.rsi/monitoring.png" },
        { "SecurityIconReleased", "/Textures/White/Interface/securityhud.rsi/released.png" },
        { "SecurityIconSearch", "/Textures/White/Interface/securityhud.rsi/search.png" },
        { "CriminalRecordIconRemove", "/Textures/White/Interface/securityhud.rsi/remove.png" }
    };

    private readonly Dictionary<string, SecurityStatus> _status = new()
    {
        { "SecurityIconDemote", SecurityStatus.Demote },
        { "SecurityIconParoled", SecurityStatus.Paroled },
        { "SecurityIconSuspected", SecurityStatus.Suspected },
        { "SecurityIconWanted", SecurityStatus.Wanted },
        { "SecurityIconIncarcerated", SecurityStatus.Detained },
        { "SecurityIconExecute", SecurityStatus.Execute },
        { "SecurityIconMonitoring", SecurityStatus.Monitoring },
        { "SecurityIconReleased", SecurityStatus.Released },
        { "SecurityIconSearch", SecurityStatus.Search }, // WD end
        { "CriminalRecordIconRemove", SecurityStatus.None }
    };


    public SecurityHudBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
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

    private void PopulateRadial(IReadOnlyCollection<string> ids, NetEntity user, NetEntity target)
    {
        foreach (var id in ids)
        {
            if (_radialContainer == null)
                continue;

            if(!_names.TryGetValue(id, out var name) || !_icons.TryGetValue(id, out var icon) || !_status.TryGetValue(id, out var status))
                return;

            var button = _radialContainer.AddButton(name, icon);
            button.Controller.OnPressed += _ =>
            {
                Select(status, user, target);
            };
        }
    }

    private void Select(SecurityStatus status, NetEntity user, NetEntity target)
    {
        SendMessage(new SecurityHudStatusSelectedMessage(status, user, target));
        UIReset();
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        UIReset();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_updated)
            return;

        if (state is SecurityHudBUIState newState)
        {
            PopulateRadial(newState.Ids, newState.User, newState.Target);
        }

        if (_radialContainer == null)
            return;

        _radialContainer?.OpenAttachedLocalPlayer();
        _updated = true;
    }
}
