using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Content.Server.Chat.Managers;
using Content.Shared._Miracle.Nya;
using Content.Shared._White;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Server._Miracle.Nya;

public sealed class CheatCheckSystem : EntitySystem
{
    [Dependency] private readonly ExpectedReplySystem _expectedReply = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;


    private readonly HttpClient _httpClient = new();

    private string _webhookUrl = "";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CheatCheckResponseEvent>(OnCheckResponse);

        _configuration.OnValueChanged(WhiteCVars.ACWebhook, s => _webhookUrl = s, true);
    }

    public void RequestCheck(ICommonSession player)
    {
        _expectedReply.ExpectReply<CheatCheckRequestEvent, CheatCheckResponseEvent>(
            player,
            new CheatCheckRequestEvent(),
            ProcessCheckResponse
        );
    }

    private void OnCheckResponse(CheatCheckResponseEvent ev, EntitySessionEventArgs args)
    {
        if (!_expectedReply.HandleReply(ev, args))
            return;
    }

    private async void ProcessCheckResponse(CheatCheckResponseEvent ev, EntitySessionEventArgs args)
    {
        var detections = new List<(string Type, string Details, int Severity)>();

        if (ev.HasPatchMetadata)
            detections.Add(("Инъекция кода", "Обнаружены метаданные патча", 90));

        if (ev.ReflectionOffender != null)
            detections.Add(("Рефлексия", $"Найден подозрительный тип: {ev.ReflectionOffender}", 80));

        if (ev.HasMoonyware)
            detections.Add(("Чит-клиент", "Обнаружен Moonyware", 95));

        if (ev.IoCOffender != null)
            detections.Add(("IoC манипуляция", $"Неразрешенный тип: {ev.IoCOffender}", 70));

        if (ev.ExtraModuleOffender != null)
            detections.Add(("Внешний модуль", $"Неразрешенный модуль: {ev.ExtraModuleOffender}", 85));

        if (ev.CvarOffender != null)
            detections.Add(("Подозрительный CVar", $"Найден чит-квар: {ev.CvarOffender}", 60));

        if (ev.SystemOffender != null)
            detections.Add(("Системное вмешательство", $"Неразрешенная система: {ev.SystemOffender}", 75));

        if (ev.ComponentOffender != null)
            detections.Add(("Компонентное вмешательство", $"Неразрешенный компонент: {ev.ComponentOffender}", 75));

        if (ev.WindowOffender != null)
            detections.Add(("UI вмешательство", $"Неразрешенное окно: {ev.WindowOffender}", 65));

        if (detections.Count == 0)
            return;

        var maxSeverity = detections.Max(d => d.Severity);
        var avgSeverity = detections.Average(d => d.Severity);
        var totalSeverity = (int)((maxSeverity * 0.7) + (avgSeverity * 0.3));

        var warningMsg = $"🚨 **Античит обнаружил подозрительную активность!**\n\n" +
                        $"**Игрок:** {args.SenderSession.Name}\n" +
                        $"**IP:** {args.SenderSession.Channel.RemoteEndPoint}\n" +
                        $"**Вероятность использования читов:** {totalSeverity}%\n\n" +
                        $"**Обнаруженные нарушения:**\n";

        foreach (var (type, details, severity) in detections)
        {
            warningMsg += $"• **{type}** ({severity}%): {details}\n";
        }

        var color = totalSeverity switch
        {
            >= 90 => 0xFF0000, // Красный
            >= 70 => 0xFFA500, // Оранжевый
            _ => 0xFFFF00 // Желтый
        };

        var embed = new
        {
            title = "🚫 Обнаружен читер!",
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

        var inGameMsg = $"[Anticheat] Обнаружена подозрительная активность!\n" +
                       $"Игрок: {args.SenderSession.Name}\n" +
                       $"Вероятность использования читов: {totalSeverity}%\n" +
                       $"Обнаруженные нарушения:";

        foreach (var (type, details, severity) in detections)
        {
            inGameMsg += $"\n•{type} ({severity}%): {details}";
        }

        _chatManager.SendAdminAnnouncement(inGameMsg);
    }
}
