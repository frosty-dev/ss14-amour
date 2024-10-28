using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Content.Shared._Miracle.Nya;
using Content.Shared._White;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Server._Miracle.Nya;

public sealed class NyaGrabSystem : EntitySystem
{
    [Dependency] private readonly ExpectedReplySystem _expectedReply = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;

    private readonly HttpClient _httpClient = new();

    private string _webhookUrl = "";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<ScreengrabResponseEvent>(OnScreengrabResponse);

        _configuration.OnValueChanged(WhiteCVars.ACWebhook, s => _webhookUrl = s, true);
    }

    public void RequestScreengrab(ICommonSession player)
    {
        _expectedReply.ExpectReply<ScreengrabRequestEvent, ScreengrabResponseEvent>(
            player,
            new ScreengrabRequestEvent(),
            OnScreengrabReply
        );
    }

    private void OnScreengrabResponse(ScreengrabResponseEvent ev, EntitySessionEventArgs args)
    {
        if (!_expectedReply.HandleReply(ev, args))
            return;
    }

    private async void OnScreengrabReply(ScreengrabResponseEvent ev, EntitySessionEventArgs args)
    {
        if (ev.Screengrab.Length == 0)
            return;

        var timestamp = DateTime.UtcNow;
        var imagedata = ev.Screengrab;
        using var image = Image.Load<Rgb24>(imagedata);

        var content = new MultipartFormDataContent();

        var fileName = $"screengrab_{args.SenderSession.UserId}_{timestamp:yyyy-MM-dd_HH-mm-ss}.jpg";
        var fileContent = new ByteArrayContent(imagedata);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", fileName);

        var embed = new
        {
            title = "📸 Скриншот игрока",
            description = $"**Игрок**: {args.SenderSession.Name}\n" +
                         $"**UserId**: {args.SenderSession.UserId}\n" +
                         $"**IP**: {args.SenderSession.Channel.RemoteEndPoint}\n" +
                         $"**Дата и время**: {timestamp:yyyy-MM-dd HH:mm:ss} UTC\n" +
                         $"**Разрешение**: {image.Width}x{image.Height}\n" +
                         $"**Размер**: {(imagedata.Length / 1024.0):F2} KB",
            color = 0x00FF00,
            timestamp = timestamp.ToString("o")
        };

        var payload = new
        {
            embeds = new[] { embed }
        };

        var jsonContent = JsonSerializer.Serialize(payload);
        content.Add(new StringContent(jsonContent), "payload_json");

        try
        {
            await _httpClient.PostAsync(_webhookUrl, content);
            Log.Info($"Screenshot sent to Discord for player {args.SenderSession.Name}");
        }
        catch (Exception e)
        {
            Log.Error($"Failed to send screenshot to Discord: {e}");
        }
    }
}
