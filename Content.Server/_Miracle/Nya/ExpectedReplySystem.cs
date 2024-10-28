using Content.Server.Chat.Managers;
using Robust.Shared.Player;
using Content.Shared._Miracle.Nya;
using Robust.Shared.Enums;
using Robust.Shared.Timing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Content.Shared._White;
using Robust.Shared.Configuration;

namespace Content.Server._Miracle.Nya;

public sealed class ExpectedReplySystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _playMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly CheatCheckSystem _cheatCheckSystem = default!;

    private readonly Dictionary<ICommonSession, PendingReply> _pendingReplies = new();

    private const float ReplyTimeoutSeconds = 5.0f;
    private readonly HttpClient _httpClient = new();

    private string _webhookUrl = "";

    public override void Initialize()
    {
        base.Initialize();
        _playMan.PlayerStatusChanged += OnPlayerStatusChanged;

        _configuration.OnValueChanged(WhiteCVars.ACWebhook, s => _webhookUrl = s, true);
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e is { OldStatus: SessionStatus.InGame, NewStatus: SessionStatus.Disconnected })
        {
            if (_pendingReplies.ContainsKey(e.Session))
            {
                var warningMsg = $"Игрок отключился во время ожидания ответа!";
                SendSuspiciousActivityAlert(e.Session, warningMsg, 80);
                _pendingReplies.Remove(e.Session);
            }
        }

        if (e.NewStatus == SessionStatus.Connected)
        {
            _cheatCheckSystem.RequestCheck(e.Session);
        }
    }

    public void ExpectReply<TRequest, TResponse>(
        ICommonSession player,
        TRequest request,
        Action<TResponse, EntitySessionEventArgs> handler)
        where TRequest : ExpectedReplyEntityEventArgs
        where TResponse : EntityEventArgs
    {
        var timeout = _timing.CurTime + TimeSpan.FromSeconds(ReplyTimeoutSeconds);

        void WrapHandler(EntityEventArgs ev, EntitySessionEventArgs args)
        {
            if (ev is TResponse response)
                handler(response, args);
        }

        _pendingReplies[player] = new PendingReply(request, timeout, WrapHandler);
        RaiseNetworkEvent(request, player.Channel);
    }

    public bool HandleReply(EntityEventArgs ev, EntitySessionEventArgs args)
    {
        if (!_pendingReplies.TryGetValue(args.SenderSession, out var pending))
        {
            var warningMsg = "Получен неожиданный ответ без запроса";
            SendSuspiciousActivityAlert(args.SenderSession, warningMsg, 70);
            return false;
        }

        if (pending.Request.ExpectedReplyType != ev.GetType())
        {
            var warningMsg = $"Получен ответ неверного типа. Ожидался {pending.Request.ExpectedReplyType}, получен {ev.GetType()}";
            SendSuspiciousActivityAlert(args.SenderSession, warningMsg, 75);
            return false;
        }

        pending.Handler(ev, args);
        _pendingReplies.Remove(args.SenderSession);
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;
        var timeoutPlayers = new List<ICommonSession>();

        foreach (var (player, pending) in _pendingReplies)
        {
            if (currentTime > pending.TimeoutTime)
                timeoutPlayers.Add(player);
        }

        foreach (var player in timeoutPlayers)
        {
            HandleTimeout(player);
            _pendingReplies.Remove(player);
        }
    }

    private void HandleTimeout(ICommonSession player)
    {
        var warningMsg = $"Не получен ответ в течение {ReplyTimeoutSeconds} секунд";
        SendSuspiciousActivityAlert(player, warningMsg, 65);
    }

    private async void SendSuspiciousActivityAlert(ICommonSession player, string reason, int severity)
    {
        var color = severity switch
        {
            >= 80 => 0xFF0000, // Красный
            >= 70 => 0xFFA500, // Оранжевый
            _ => 0xFFFF00 // Желтый
        };

        var warningMsg = $"⚠️ **Система ожидаемых ответов обнаружила подозрительную активность!**\n\n" +
                         $"**Игрок:** {player.Name}\n" +
                         $"**IP:** {player.Channel.RemoteEndPoint}\n" +
                         $"**Уровень подозрительности:** {severity}%\n" +
                         $"**Причина:** {reason}";

        var embed = new
        {
            title = "⚠️ Подозрительная активность!",
            description = warningMsg,
            color = color,
            timestamp = DateTime.UtcNow.ToString("o")
        };

        var payload = new
        {
            embeds = new[] { embed }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            await _httpClient.PostAsync(_webhookUrl, content);
        }
        catch (Exception e)
        {
            Log.Error($"Failed to send Discord webhook: {e}");
        }

        var inGameMsg = $"[Anticheat] Внимание! Подозрительная активность:\n" +
                        $"Игрок {player.Name} возможно читер!\n" +
                        $"Причина обнаружения: {reason}";

        _chatManager.SendAdminAnnouncement(inGameMsg);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _playMan.PlayerStatusChanged -= OnPlayerStatusChanged;
        _pendingReplies.Clear();
    }
}
