using Content.Shared._White.Cult.Components;
using Content.Shared.Examine;

namespace Content.Shared._White.Cult.Systems;

public sealed class DeleteOnDropAttemptSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeleteOnDropAttemptComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<DeleteOnDropAttemptComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.Message));
    }
}
