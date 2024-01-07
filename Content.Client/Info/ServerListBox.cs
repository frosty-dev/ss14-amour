using Robust.Client;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.Info
{
    public sealed class ServerListBox : BoxContainer
    {
        private IGameController _gameController;
        private List<Button> _connectButtons = new();
        private IUriOpener _uriOpener;

        public ServerListBox()
        {
            _gameController = IoCManager.Resolve<IGameController>();
            _uriOpener = IoCManager.Resolve<IUriOpener>();
            Orientation = LayoutOrientation.Vertical;
            AddServers();
        }

        private void AddServers()
        {
            AddServerInfo("Grid", "Сервер с постоянным безумием", "ss14://ludere.ss14.su/grid", null);
            AddServerInfo("Maid", "Сервер с лучшим сервисом", "ss14://ludere.ss14.su/maid", null);
            AddServerInfo("Engi", "Сервер с расслабленным геймплеем", "ss14://ludere.ss14.su/engi", null);
            AddServerInfo("Amour", "Сервер для любителей ЕРП", "ss14://ludere.ss14.su/amour", "https://discord.gg/vxHPsZ3Qyr");
            AddServerInfo("Glasio", "Сервер с лучшим отыгрышем", "ss14://ludere.ss14.su/glasio", "https://discord.gg/TGzZep96cR");
            AddServerInfo("Ataraxia", "Для любителей ролевой игры", "ss14://ludere.ss14.su/ataraxia", "https://discord.gg/BCPkP3TcDT");
        }

        private void AddServerInfo(string serverName, string description, string serverUrl, string ?discord)
        {
            var serverBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
            };

            var nameAndDescriptionBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
            };

            var serverNameLabel = new Label
            {
                Text = serverName,
                MinWidth = 200
            };

            var descriptionLabel = new RichTextLabel
            {
                MaxWidth = 500
            };
            descriptionLabel.SetMessage(FormattedMessage.FromMarkup(description));

            var buttonBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalExpand = true,
                HorizontalAlignment = HAlignment.Right
            };

            var connectButton = new Button
            {
                Text = "Подключиться"
            };

            if (discord != null)
            {
                var discordButton = new Button
                {
                    Text = "Discord"
                };

                discordButton.OnPressed += _ =>
                {
                    _uriOpener.OpenUri(discord);
                };

                buttonBox.AddChild(discordButton);
            }

            _connectButtons.Add(connectButton);

            connectButton.OnPressed += _ =>
            {
                _gameController.Redial(serverUrl, "Connecting to another server...");

                foreach (var connectButton in _connectButtons)
                {
                    connectButton.Disabled = true;
                }
            };

            buttonBox.AddChild(connectButton);

            nameAndDescriptionBox.AddChild(serverNameLabel);
            nameAndDescriptionBox.AddChild(descriptionLabel);

            serverBox.AddChild(nameAndDescriptionBox);
            serverBox.AddChild(buttonBox);

            AddChild(serverBox);
        }
    }
}
