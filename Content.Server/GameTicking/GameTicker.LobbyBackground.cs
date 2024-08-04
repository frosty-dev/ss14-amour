using Robust.Shared.Random;
using System.Linq;
using Content.Shared._White;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    [ViewVariables]
    public string? LobbyBackground { get; private set; }

    [ViewVariables]
    private List<string>? _lobbyBackgrounds;

    private void InitializeLobbyBackground()
    {
        _lobbyBackgrounds = _prototypeManager.EnumeratePrototypes<AnimatedLobbyScreenPrototype>()
            .Select(x => x.Path)
            .ToList();

        RandomizeLobbyBackground();
    }

    private void RandomizeLobbyBackground()
    {
        LobbyBackground = _lobbyBackgrounds!.Any() ? _robustRandom.Pick(_lobbyBackgrounds!) : null;
    }
}
