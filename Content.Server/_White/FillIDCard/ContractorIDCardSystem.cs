
using Content.Shared.Examine;

namespace Content.Server._White.ContractorIDCard;

public sealed class ContractorIDCardSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContractorIDCardComponent, ExaminedEvent>(OnExamined);
    }
    public void OnExamined(Entity<ContractorIDCardComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (ent.Comp.Details == string.Empty)
            return;

        args.PushMarkup(ent.Comp.Details);
    }
}
