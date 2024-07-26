using Content.Shared._White.Cult.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;

namespace Content.Shared._White.Cult.Systems;

public sealed class ConcealableSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConcealableComponent, ExamineAttemptEvent>(OnExamine);
        SubscribeLocalEvent<ConcealableComponent, GettingInteractedWithAttemptEvent>(OnInteract);
    }

    private void OnInteract(Entity<ConcealableComponent> ent, ref GettingInteractedWithAttemptEvent args)
    {
        if (ent.Comp is {Concealed: true, ExaminableWhileConcealed: false})
            args.Cancel();
    }

    private void OnExamine(Entity<ConcealableComponent> ent, ref ExamineAttemptEvent args)
    {
        if (ent.Comp is {Concealed: true, ExaminableWhileConcealed: false})
            args.Cancel();
    }
}

public sealed class ConcealEvent : EntityEventArgs
{
    public bool Conceal;

    public ConcealEvent(bool conceal)
    {
        Conceal = conceal;
    }
}
