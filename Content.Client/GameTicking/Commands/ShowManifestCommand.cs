using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Client.RoundEnd;
using Robust.Client.UserInterface;

namespace Content.Client.GameTicking.Commands
{
    [AnyCommand]
    public sealed class ShowManifestCommand : IConsoleCommand
    {
        [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
        public string Command => "showmanifest";
        public string Description => "Shows round end summary window";
        public string Help => "Usage: showmanifest";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            _userInterfaceManager.GetUIController<RoundEndSummaryUIController>().ShowManifest();
        }
    }
}
