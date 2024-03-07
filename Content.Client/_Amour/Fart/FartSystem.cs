using Content.Client.Gameplay;
using Robust.Client.Audio;
using Robust.Client.ResourceManagement;
using Robust.Client.State;

namespace Content.Client._Amour.Fart;

public sealed class FartSystem : EntitySystem
{
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly IAudioManager _audioManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public const string FartPath = "/Audio/_Amour/fart-with-reverb.ogg";
    private AudioResource _audioResource = default!;

    public override void Initialize()
    {
        _audioResource = _resourceCache.GetResource<AudioResource>(FartPath);
        _stateManager.OnStateChanged += StateManagerOnOnStateChanged;
    }

    private void StateManagerOnOnStateChanged(StateChangedEventArgs obj)
    {
        if(obj.OldState is not GameplayState state)
            return;

        _audioManager.CreateAudioSource(_audioResource.AudioStream)?.StartPlaying();
    }
}
