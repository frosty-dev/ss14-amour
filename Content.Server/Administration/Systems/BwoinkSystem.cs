using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Content.Server._Miracle.GulagSystem;
using Content.Server.Administration.Managers;
using Content.Server.Afk;
using Content.Server.GameTicking;
using Content.Server._White.PandaSocket.Main;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Mind;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Systems
{
    [UsedImplicitly]
    public sealed partial class BwoinkSystem : SharedBwoinkSystem
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IPlayerLocator _playerLocator = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly SharedMindSystem _minds = default!;
        [Dependency] private readonly PandaWebManager _pandaWeb = default!; // WD
        [Dependency] private readonly GulagSystem _gulagSystem = default!; // Miracle
        [Dependency] private readonly IAfkManager _afkManager = default!;

        [GeneratedRegex(@"^https://discord\.com/api/webhooks/(\d+)/((?!.*/).*)$")]
        private static partial Regex DiscordRegex();

        private ISawmill _sawmill = default!;
        private readonly HttpClient _httpClient = new();
        private string _webhookUrl = string.Empty;
        private WebhookData? _webhookData;
        private string _footerIconUrl = string.Empty;
        private string _avatarUrl = string.Empty;
        private string _serverName = string.Empty;
        private readonly Dictionary<NetUserId, (string? id, string username, string description, string? characterName, GameRunLevel lastRunLevel)> _relayMessages = new();
        private Dictionary<NetUserId, string> _oldMessageIds = new();
        private readonly Dictionary<NetUserId, Queue<string>> _messageQueues = new();
        private readonly HashSet<NetUserId> _processingChannels = new();
        private readonly Dictionary<NetUserId, (TimeSpan Timestamp, bool Typing)> _typingUpdateTimestamps = new();
        private string _overrideClientName = string.Empty;

        // Max embed description length is 4096, according to https://discord.com/developers/docs/resources/channel#embed-object-embed-limits
        // Keep small margin, just to be safe
        private const ushort DescriptionMax = 4000;

        // Maximum length a message can be before it is cut off
        // Should be shorter than DescriptionMax
        private const ushort MessageLengthCap = 3000;

        // Text to be used to cut off messages that are too long. Should be shorter than MessageLengthCap
        private const string TooLongText = "... **(too long)**";

        private int _maxAdditionalChars;

        public override void Initialize()
        {
            base.Initialize();
            Subs.CVar(_config, CCVars.DiscordAHelpWebhook, OnWebhookChanged, true);
            Subs.CVar(_config, CCVars.DiscordAHelpFooterIcon, OnFooterIconChanged, true);
            Subs.CVar(_config, CCVars.DiscordAHelpAvatar, OnAvatarChanged, true);
            Subs.CVar(_config, CVars.GameHostName, OnServerNameChanged, true);
            Subs.CVar(_config, CCVars.AdminAhelpOverrideClientName, OnOverrideChanged, true);
            _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("AHELP");
            _maxAdditionalChars = GenerateAHelpMessage("", "", true, _gameTicker.RoundDuration().ToString("hh\\:mm\\:ss"), _gameTicker.RunLevel, playedSound: false).Length;
            _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

            SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChanged);
            SubscribeNetworkEvent<BwoinkClientTypingUpdated>(OnClientTypingUpdated);
        }

        private void OnOverrideChanged(string obj)
        {
            _overrideClientName = obj;
        }

        private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
        {
            if (e.NewStatus != SessionStatus.InGame)
                return;

            RaiseNetworkEvent(new BwoinkDiscordRelayUpdated(!string.IsNullOrWhiteSpace(_webhookUrl)), e.Session);
        }

        private void OnGameRunLevelChanged(GameRunLevelChangedEvent args)
        {
            // Don't make a new embed if we
            // 1. were in the lobby just now, and
            // 2. are not entering the lobby or directly into a new round.
            if (args.Old is GameRunLevel.PreRoundLobby ||
                args.New is not (GameRunLevel.PreRoundLobby or GameRunLevel.InRound))
            {
                return;
            }

            // Store the Discord message IDs of the previous round
            _oldMessageIds = new Dictionary<NetUserId, string>();
            foreach (var message in _relayMessages)
            {
                var id = message.Value.id;
                if (id == null)
                    return;

                _oldMessageIds[message.Key] = id;
            }

            _relayMessages.Clear();
        }

        private void OnClientTypingUpdated(BwoinkClientTypingUpdated msg, EntitySessionEventArgs args)
        {
            if (_typingUpdateTimestamps.TryGetValue(args.SenderSession.UserId, out var tuple) &&
                tuple.Typing == msg.Typing &&
                tuple.Timestamp + TimeSpan.FromSeconds(1) > _timing.RealTime)
            {
                return;
            }

            _typingUpdateTimestamps[args.SenderSession.UserId] = (_timing.RealTime, msg.Typing);

            // Non-admins can only ever type on their own ahelp, guard against fake messages
            var isAdmin = _adminManager.GetAdminData(args.SenderSession)?.HasFlag(AdminFlags.Adminhelp) ?? false;
            var channel = isAdmin ? msg.Channel : args.SenderSession.UserId;
            var update = new BwoinkPlayerTypingUpdated(channel, args.SenderSession.Name, msg.Typing);

            foreach (var admin in GetTargetAdmins())
            {
                if (admin.UserId == args.SenderSession.UserId)
                    continue;

                RaiseNetworkEvent(update, admin);
            }
        }

        private void OnServerNameChanged(string obj)
        {
            _serverName = obj;
        }

        private async void OnWebhookChanged(string url)
        {
            _webhookUrl = url;

            RaiseNetworkEvent(new BwoinkDiscordRelayUpdated(!string.IsNullOrWhiteSpace(url)));

            if (url == string.Empty)
                return;

            // Basic sanity check and capturing webhook ID and token
            var match = DiscordRegex().Match(url);

            if (!match.Success)
            {
                // TODO: Ideally, CVar validation during setting should be better integrated
                Log.Warning("Webhook URL does not appear to be valid. Using anyways...");
                return;
            }

            if (match.Groups.Count <= 2)
            {
                Log.Error("Could not get webhook ID or token.");
                return;
            }

            var webhookId = match.Groups[1].Value;
            var webhookToken = match.Groups[2].Value;

            // Fire and forget
            await SetWebhookData(webhookId, webhookToken);
        }

        private async Task SetWebhookData(string id, string token)
        {
            var response = await _httpClient.GetAsync($"https://discord.com/api/v10/webhooks/{id}/{token}");

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _sawmill.Log(LogLevel.Error, $"Discord returned bad status code when trying to get webhook data (perhaps the webhook URL is invalid?): {response.StatusCode}\nResponse: {content}");
                return;
            }

            _webhookData = JsonSerializer.Deserialize<WebhookData>(content);
        }

        private void OnFooterIconChanged(string url)
        {
            _footerIconUrl = url;
        }

        private void OnAvatarChanged(string url)
        {
            _avatarUrl = url;
        }

        private async void ProcessQueue(NetUserId userId, Queue<string> messages)
        {
            // Whether an embed already exists for this player
            var exists = _relayMessages.TryGetValue(userId, out var existingEmbed);

            // Whether the message will become too long after adding these new messages
            var tooLong = exists && messages.Sum(msg => Math.Min(msg.Length, MessageLengthCap) + "\n".Length)
                    + existingEmbed.description.Length > DescriptionMax;

            // If there is no existing embed, or it is getting too long, we create a new embed
            if (!exists || tooLong)
            {
                var lookup = await _playerLocator.LookupIdAsync(userId);

                if (lookup == null)
                {
                    _sawmill.Log(LogLevel.Error, $"Unable to find player for NetUserId {userId} when sending discord webhook.");
                    _relayMessages.Remove(userId);
                    return;
                }

                var linkToPrevious = string.Empty;

                // If we have all the data required, we can link to the embed of the previous round or embed that was too long
                if (_webhookData is { GuildId: { } guildId, ChannelId: { } channelId })
                {
                    if (tooLong && existingEmbed.id != null)
                    {
                        linkToPrevious = $"**[Go to previous embed of this round](https://discord.com/channels/{guildId}/{channelId}/{existingEmbed.id})**\n";
                    }
                    else if (_oldMessageIds.TryGetValue(userId, out var id) && !string.IsNullOrEmpty(id))
                    {
                        linkToPrevious = $"**[Go to last round's conversation with this player](https://discord.com/channels/{guildId}/{channelId}/{id})**\n";
                    }
                }

                var characterName = _minds.GetCharacterName(userId);
                existingEmbed = (null, lookup.Username, linkToPrevious, characterName, _gameTicker.RunLevel);
            }

            // Previous message was in another RunLevel, so show that in the embed
            if (existingEmbed.lastRunLevel != _gameTicker.RunLevel)
            {
                existingEmbed.description += _gameTicker.RunLevel switch
                {
                    GameRunLevel.PreRoundLobby => "\n\n:arrow_forward: _**Pre-round lobby started**_\n",
                    GameRunLevel.InRound => "\n\n:arrow_forward: _**Round started**_\n",
                    GameRunLevel.PostRound => "\n\n:stop_button: _**Post-round started**_\n",
                    _ => throw new ArgumentOutOfRangeException(nameof(_gameTicker.RunLevel), $"{_gameTicker.RunLevel} was not matched."),
                };

                existingEmbed.lastRunLevel = _gameTicker.RunLevel;
            }

            // Add available messages to the embed description
            while (messages.TryDequeue(out var message))
            {
                // In case someone thinks they're funny
                if (message.Length > MessageLengthCap)
                    message = message[..(MessageLengthCap - TooLongText.Length)] + TooLongText;

                existingEmbed.description += $"\n{message}";
            }

            var payload = GeneratePayload(existingEmbed.description, existingEmbed.username, existingEmbed.characterName);

            // If there is no existing embed, create a new one
            // Otherwise patch (edit) it
            if (existingEmbed.id == null)
            {
                var request = await _httpClient.PostAsync($"{_webhookUrl}?wait=true",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                var content = await request.Content.ReadAsStringAsync();
                if (!request.IsSuccessStatusCode)
                {
                    _sawmill.Log(LogLevel.Error, $"Discord returned bad status code when posting message (perhaps the message is too long?): {request.StatusCode}\nResponse: {content}");
                    _relayMessages.Remove(userId);
                    return;
                }

                var id = JsonNode.Parse(content)?["id"];
                if (id == null)
                {
                    _sawmill.Log(LogLevel.Error, $"Could not find id in json-content returned from discord webhook: {content}");
                    _relayMessages.Remove(userId);
                    return;
                }

                existingEmbed.id = id.ToString();
            }
            else
            {
                var request = await _httpClient.PatchAsync($"{_webhookUrl}/messages/{existingEmbed.id}",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                if (!request.IsSuccessStatusCode)
                {
                    var content = await request.Content.ReadAsStringAsync();
                    _sawmill.Log(LogLevel.Error, $"Discord returned bad status code when patching message (perhaps the message is too long?): {request.StatusCode}\nResponse: {content}");
                    _relayMessages.Remove(userId);
                    return;
                }
            }

            _relayMessages[userId] = existingEmbed;

            _processingChannels.Remove(userId);
        }

        private WebhookPayload GeneratePayload(string messages, string username, string? characterName = null)
        {
            // Add character name
            if (characterName != null)
                username += $" ({characterName})";

            // If no admins are online, set embed color to red. Otherwise green
            var color = GetNonAfkAdmins().Count > 0 ? 0x41F097 : 0xFF0000;

            // Limit server name to 1500 characters, in case someone tries to be a little funny
            var serverName = _serverName[..Math.Min(_serverName.Length, 1500)];

            var round = _gameTicker.RunLevel switch
            {
                GameRunLevel.PreRoundLobby => _gameTicker.RoundId == 0
                    ? "pre-round lobby after server restart" // first round after server restart has ID == 0
                    : $"pre-round lobby for round {_gameTicker.RoundId + 1}",
                GameRunLevel.InRound => $"round {_gameTicker.RoundId}",
                GameRunLevel.PostRound => $"post-round {_gameTicker.RoundId}",
                _ => throw new ArgumentOutOfRangeException(nameof(_gameTicker.RunLevel), $"{_gameTicker.RunLevel} was not matched."),
            };

            return new WebhookPayload
            {
                Username = username,
                AvatarUrl = string.IsNullOrWhiteSpace(_avatarUrl) ? null : _avatarUrl,
                Embeds = new List<Embed>
                {
                    new()
                    {
                        Description = messages,
                        Color = color,
                        Footer = new EmbedFooter
                        {
                            Text = $"{serverName} ({round})",
                            IconUrl = string.IsNullOrWhiteSpace(_footerIconUrl) ? null : _footerIconUrl
                        },
                    },
                },
            };
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var userId in _messageQueues.Keys.ToArray())
            {
                if (_processingChannels.Contains(userId))
                    continue;

                var queue = _messageQueues[userId];
                _messageQueues.Remove(userId);
                if (queue.Count == 0)
                    continue;

                _processingChannels.Add(userId);

                ProcessQueue(userId, queue);
            }
        }

        protected override void OnBwoinkTextMessage(BwoinkTextMessage message, EntitySessionEventArgs eventArgs)
        {
            base.OnBwoinkTextMessage(message, eventArgs);
            var senderSession = eventArgs.SenderSession;

            //No bwoink for banned
            if (_gulagSystem.IsUserGulagged(senderSession.UserId, out _))
            {
                return;
            }

            // TODO: Sanitize text?
            // Confirm that this person is actually allowed to send a message here.
            var personalChannel = senderSession.UserId == message.UserId;
            var senderAdmin = _adminManager.GetAdminData(senderSession);
            var senderAHelpAdmin = senderAdmin?.HasFlag(AdminFlags.Adminhelp) ?? false;
            var authorized = personalChannel || senderAHelpAdmin;
            if (!authorized)
            {
                // Unauthorized bwoink (log?)
                return;
            }

            var escapedText = FormattedMessage.EscapeText(message.Text);

            string bwoinkText;
            string adminPrefix = "";

            //Getting an administrator position
            if (_config.GetCVar(CCVars.AhelpAdminPrefix) && senderAdmin is not null && senderAdmin.Title is not null)
            {
                adminPrefix = $"[bold]\\[{senderAdmin.Title}\\][/bold] ";
            }

            if (senderAdmin is not null && senderAdmin.Flags == AdminFlags.Adminhelp) // Mentor. Not full admin. That's why it's colored differently.
            {
                bwoinkText = $"[color=purple]{adminPrefix}{senderSession.Name}[/color]";
            }
            else if (senderAdmin is not null && senderAdmin.HasFlag(AdminFlags.Adminhelp))
            {
                bwoinkText = $"[color=red]{adminPrefix}{senderSession.Name}[/color]";
            }
            else
            {
                bwoinkText = $"{senderSession.Name}";
            }

            bwoinkText = $"{(message.PlaySound ? "" : "(S) ")}{bwoinkText}: {escapedText}";

            // If it's not an admin / admin chooses to keep the sound then play it.
            var playSound = !senderAHelpAdmin || message.PlaySound;
            var msg = new BwoinkTextMessage(message.UserId, senderSession.UserId, bwoinkText, senderAHelpAdmin, playSound: playSound);

            LogBwoink(msg);

            var admins = GetTargetAdmins();

            // Notify all admins
            foreach (var channel in admins)
            {
                RaiseNetworkEvent(msg, channel);
            }

            string adminPrefixWebhook = "";

            if (_config.GetCVar(CCVars.AhelpAdminPrefixWebhook) && senderAdmin is not null && senderAdmin.Title is not null)
            {
                adminPrefixWebhook = $"[bold]\\[{senderAdmin.Title}\\][/bold] ";
            }

            // Notify player
            if (_playerManager.TryGetSessionById(message.UserId, out var session))
            {
                if (!admins.Contains(session.Channel))
                {
                    // If _overrideClientName is set, we generate a new message with the override name. The admins name will still be the original name for the webhooks.
                    if (_overrideClientName != string.Empty)
                    {
                        string overrideMsgText;
                        // Doing the same thing as above, but with the override name. Theres probably a better way to do this.
                        if (senderAdmin is not null && senderAdmin.Flags == AdminFlags.Adminhelp) // Mentor. Not full admin. That's why it's colored differently.
                        {
                            overrideMsgText = $"[color=purple]{adminPrefixWebhook}{_overrideClientName}[/color]";
                        }
                        else if (senderAdmin is not null && senderAdmin.HasFlag(AdminFlags.Adminhelp))
                        {
                            overrideMsgText = $"[color=red]{adminPrefixWebhook}{_overrideClientName}[/color]";
                        }
                        else
                        {
                            overrideMsgText = $"{senderSession.Name}"; // Not an admin, name is not overridden.
                        }

                        overrideMsgText = $"{(message.PlaySound ? "" : "(S) ")}{overrideMsgText}: {escapedText}";

                        RaiseNetworkEvent(new BwoinkTextMessage(message.UserId, senderSession.UserId, overrideMsgText, senderAHelpAdmin, playSound: playSound), session.Channel);
                    }
                    else
                        RaiseNetworkEvent(msg, session.Channel);
                }
            }

            var sendsWebhook = _webhookUrl != string.Empty;
            if (sendsWebhook)
            {
                if (!_messageQueues.ContainsKey(msg.UserId))
                    _messageQueues[msg.UserId] = new Queue<string>();

                var str = message.Text;
                var unameLength = senderSession.Name.Length;

                if (unameLength + str.Length + _maxAdditionalChars > DescriptionMax)
                {
                    str = str[..(DescriptionMax - _maxAdditionalChars - unameLength)];
                }
                var nonAfkAdmins = GetNonAfkAdmins();
                _messageQueues[msg.UserId].Enqueue(GenerateAHelpMessage(senderSession.Name, str, !personalChannel, _gameTicker.RoundDuration().ToString(@"hh\:mm\:ss"), _gameTicker.RunLevel, playedSound: playSound, noReceivers: nonAfkAdmins.Count == 0));
            }

            // WD start
            var utkaCkey = _playerManager.GetSessionById(message.UserId).Channel.UserName;
            var utkaSender = _playerManager.GetSessionById(senderSession.UserId).Channel.UserName;
            UtkaSendAhelpPm(message.Text, utkaCkey, utkaSender);
            // WD end

            if (admins.Count != 0 || sendsWebhook)
                return;

            // No admin online, let the player know
            var systemText = Loc.GetString("bwoink-system-starmute-message-no-other-users");
            var starMuteMsg = new BwoinkTextMessage(message.UserId, SystemUserId, systemText, senderAHelpAdmin);
            RaiseNetworkEvent(starMuteMsg, senderSession.Channel);
        }

        private IList<INetChannel> GetNonAfkAdmins()
        {
            return _adminManager.ActiveAdmins
                .Where(p => (_adminManager.GetAdminData(p)?.HasFlag(AdminFlags.Adminhelp) ?? false) && !_afkManager.IsAfk(p))
                .Select(p => p.Channel)
                .ToList();
        }

        private IList<INetChannel> GetTargetAdmins()
        {
            return _adminManager.ActiveAdmins
                .Where(p => _adminManager.GetAdminData(p)?.HasFlag(AdminFlags.Adminhelp) ?? false)
                .Select(p => p.Channel)
                .ToList();
        }

        private static string GenerateAHelpMessage(string username, string message, bool admin, string roundTime, GameRunLevel roundState, bool playedSound, bool noReceivers = false)
        {
            var stringbuilder = new StringBuilder();

            if (admin)
                stringbuilder.Append(":outbox_tray:");
            else if (noReceivers)
                stringbuilder.Append(":sos:");
            else
                stringbuilder.Append(":inbox_tray:");

            if(roundTime != string.Empty && roundState == GameRunLevel.InRound)
                stringbuilder.Append($" **{roundTime}**");
            if (!playedSound)
                stringbuilder.Append(" **(S)**");
            stringbuilder.Append($" **{username}:** ");
            stringbuilder.Append(message);
            return stringbuilder.ToString();
        }

        // https://discord.com/developers/docs/resources/channel#message-object-message-structure
        private struct WebhookPayload
        {
            [JsonPropertyName("username")]
            public string Username { get; set; } = "";

            [JsonPropertyName("avatar_url")]
            public string? AvatarUrl { get; set; } = "";

            [JsonPropertyName("embeds")]
            public List<Embed>? Embeds { get; set; } = null;

            [JsonPropertyName("allowed_mentions")]
            public Dictionary<string, string[]> AllowedMentions { get; set; } =
                new()
                {
                    { "parse", Array.Empty<string>() },
                };

            public WebhookPayload()
            {
            }
        }

        // https://discord.com/developers/docs/resources/channel#embed-object-embed-structure
        private struct Embed
        {
            [JsonPropertyName("description")]
            public string Description { get; set; } = "";

            [JsonPropertyName("color")]
            public int Color { get; set; } = 0;

            [JsonPropertyName("footer")]
            public EmbedFooter? Footer { get; set; } = null;

            public Embed()
            {
            }
        }

        // https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure
        private struct EmbedFooter
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = "";

            [JsonPropertyName("icon_url")]
            public string? IconUrl { get; set; }

            public EmbedFooter()
            {
            }
        }

        // https://discord.com/developers/docs/resources/webhook#webhook-object-webhook-structure
        private struct WebhookData
        {
            [JsonPropertyName("guild_id")]
            public string? GuildId { get; set; } = null;

            [JsonPropertyName("channel_id")]
            public string? ChannelId { get; set; } = null;

            public WebhookData()
            {
            }
        }

        //WD-EDIT
        public void SendUtkaBwoinkMessage(NetUserId receiver, string sender, string text)
        {
            var bwoinkText = $"[color=red](D) {sender}[/color]: {text}";
            _playerManager.TryGetUserId(sender, out var senderId);
            var msg = new BwoinkTextMessage(receiver, senderId, bwoinkText, true);

            LogBwoink(msg);

            var admins = GetTargetAdmins();

            // Notify all admins
            foreach (var channel in admins)
            {
                RaiseNetworkEvent(msg, channel);
            }

            // Notify player
            if (_playerManager.TryGetSessionById(receiver, out var session))
            {
                if (!admins.Contains(session.Channel))
                    RaiseNetworkEvent(msg, session.Channel);
            }

            var sendsWebhook = _webhookUrl != string.Empty;
            if (sendsWebhook)
            {
                if (!_messageQueues.ContainsKey(msg.UserId))
                    _messageQueues[msg.UserId] = new Queue<string>();

                var str = text;
                var unameLength = sender.Length;

                if (unameLength + str.Length + _maxAdditionalChars > DescriptionMax)
                {
                    str = str[..(DescriptionMax - _maxAdditionalChars - unameLength)];
                }
                _messageQueues[msg.UserId].Enqueue(GenerateAHelpMessage(sender, str, true,
                    _gameTicker.RoundDuration().ToString(@"hh\:mm\:ss"), _gameTicker.RunLevel, false));
            }

            var utkaCkey = _playerManager.GetSessionById(receiver).Channel.UserName;
            UtkaSendAhelpPm(text, utkaCkey, sender);
        }

        private void UtkaSendAhelpPm(string message, string ckey, string sender)
        {
            var adminManager = IoCManager.Resolve<IAdminManager>();
            var admins = adminManager.ActiveAdmins.Any();

            var entity = ckey;

            if (!_playerManager.TryGetSessionByUsername(ckey, out var session))
                return;

            if (session.AttachedEntity != null)
            {
                var meta = MetaData(session.AttachedEntity.Value);
                entity = meta.EntityName;
            }

            var utkaAhelpEvent = new UtkaAhelpPmEvent
            {
                Message = message,
                Ckey = ckey,
                Sender = sender,
                Rid = _gameTicker.RoundId,
                NoAdmins = !admins,
                Entity = entity
            };

            _pandaWeb.SendBotPostMessage(utkaAhelpEvent);
        }
        //WD-EDIT
    }
}

