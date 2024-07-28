using Content.Shared._White.Cult.Components;

namespace Content.Shared._White.Cult.Systems;

/// <summary>
/// Thats need for chat perms update
/// </summary>
public sealed class CultistSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<ConstructComponent, ComponentShutdown>(OnRemove);
        SubscribeLocalEvent<CultistComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<CultistComponent, ComponentShutdown>(OnRemove);
    }

    private void OnInit<T>(EntityUid uid, T component, ComponentStartup args)
    {
        RaiseLocalEvent(new EventCultistComponentState(true));
    }

    private void OnRemove<T>(EntityUid uid, T component, ComponentShutdown args)
    {
        RaiseLocalEvent(new EventCultistComponentState(false));
    }
}

public sealed class EventCultistComponentState
{
    public bool Created { get; }
    public EventCultistComponentState(bool state)
    {
        Created = state;
    }
}
