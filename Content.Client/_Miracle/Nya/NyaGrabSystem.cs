using System.IO;
using Content.Shared._Miracle.Nya;
using Robust.Client.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Client._Miracle.Nya;

public sealed class NyaGrabSystem : EntitySystem
{
    [Dependency] private readonly IClyde _clyde = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ScreengrabRequestEvent>(OnScreengrabRequest);
    }

    private async void OnScreengrabRequest(ScreengrabRequestEvent ev)
    {
        var image = await _clyde.ScreenshotAsync(ScreenshotType.Final);
        var array = ImageToByteArray(image);

        if (array.Length > 1_500_000)
            return;

        var msg = new ScreengrabResponseEvent { Screengrab = array };
        RaiseNetworkEvent(msg);
    }

    private byte[] ImageToByteArray(Image<Rgb24> image)
    {
        using var stream = new MemoryStream();
        image.SaveAsJpeg(stream);
        return stream.ToArray();
    }
}
