using System.Linq;
using Content.Shared._White;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Ohio.UI;

public sealed class AnimatedBackgroundControl : TextureRect
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private string _rsiPath = "/Textures/White/Lobby/backgrounds/native.rsi";
    public RSI? _RSI;
    private const int States = 1;

    private readonly BackgroundData[] _data = new BackgroundData[States];

    public AnimatedBackgroundControl()
    {
        IoCManager.InjectDependencies(this);

        InitializeStates();
    }

    private void InitializeStates()
    {
        _RSI ??= _resourceCache.GetResource<RSIResource>(_rsiPath).RSI;

        for (var i = 0; i < States; i++)
        {
            if (!_RSI.TryGetState((i + 1).ToString(), out var state))
                continue;

            var frames = state.GetFrames(RsiDirection.South);
            var delays = state.GetDelays();

            // Похуй, Linq во время инициализации можно юзать... полагаю
            var frameData = frames.Select((texture, index) => new Frame(texture, delays[index])).ToArray();
            _data[i] = new BackgroundData(frameData);
        }
    }

    public void SetRSI(RSI? rsi)
    {
        _RSI = rsi;
        InitializeStates();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        foreach (var backData in _data)
        {
            var frame = backData.Current();
            backData.Timer += args.DeltaSeconds;

            if(backData.Timer < frame.Delay)
                continue;

            backData.Timer = 0;

            Texture = frame.Texture;
            backData.Next();
        }
    }

    public void RandomizeBackground()
    {
        var backgroundsProto = _prototypeManager.EnumeratePrototypes<AnimatedLobbyScreenPrototype>().ToList();
        var random = new Random();
        var index = random.Next(backgroundsProto.Count);
        _rsiPath = $"/Textures/{backgroundsProto[index].Path}";
        InitializeStates();
    }
}

public record struct Frame(Texture Texture,float Delay);

public sealed class BackgroundData
{
    public readonly Frame[] Frames;
    public int Counter;
    public float Timer;

    public BackgroundData(Frame[] frames)
    {
        Frames = frames;
    }

    public Frame Current()
    {
        return Frames[Counter];
    }

    public void Next()
    {
        Counter = (Counter + 1) % Frames.Length;
    }
}
