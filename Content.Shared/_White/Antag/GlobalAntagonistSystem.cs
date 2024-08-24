using Content.Shared.NukeOps;

namespace Content.Shared._White.Antag;

public sealed class GlobalAntagonistSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeOperativeComponent, ComponentAdd>(OnNukeAdd);
    }

    private void OnNukeAdd (EntityUid uid, NukeOperativeComponent component, ComponentAdd _)
    {
        var antagonistComponent = EnsureComp<GlobalAntagonistComponent>(uid);
        antagonistComponent.AntagonistPrototype = "globalAntagonistNukeops";
    }
}
