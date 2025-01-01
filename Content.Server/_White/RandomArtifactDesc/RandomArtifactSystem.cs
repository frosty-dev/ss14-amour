using Content.Shared.Examine;

namespace Content.Server._White.RandomArtifactDesc;

public sealed class RandomArtifactDescSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomArtifactDescComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, RandomArtifactDescComponent component, ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
        {
            args.PushMarkup("С этим предметом что-то не так.");
        }
    }

}
