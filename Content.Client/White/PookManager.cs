using Content.Client.Gameplay;
using Content.Client.IoC;
using Content.Client.Launcher;
using Content.Client.Lobby;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Shared.GameStates;

namespace Content.Client.White;

public sealed class PookManager
{
    [Dependency] private readonly IClydeAudio _audio = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;

    public void Initialize()
    {
        _stateManager.OnStateChanged += StateManagerOnOnStateChanged;
    }

    private void StateManagerOnOnStateChanged(StateChangedEventArgs obj)
    {
        if (obj is { NewState: LauncherConnecting, OldState: LobbyState or GameplayStateBase })
        {
            var source = _audio.CreateAudioSource(StaticIoC.ResC.GetResource<AudioResource>("/Audio/White/fart-sound-effect.ogg"));
            if (source != null)
            {
                source.StartPlaying();
            }
        }
    }
}
