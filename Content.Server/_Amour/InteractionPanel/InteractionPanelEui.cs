using System.Linq;
using Content.Server.EUI;
using Content.Server.Interaction;
using Content.Shared._Amour.Arousal;
using Content.Shared._Amour.InteractionPanel;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;

namespace Content.Server._Amour.InteractionPanel;

public sealed class InteractionPanelEui : BaseEui
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public readonly Entity<InteractionPanelComponent> User;
    public readonly Entity<InteractionPanelComponent> Target;

    public InteractionPanelEui(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target)
    {
        IoCManager.InjectDependencies(this);

        User = user;
        Target = target;
    }

    public override void Opened()
    {
        base.Opened();
        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        byte? arousal = null;
        if (_entityManager.TryGetComponent<ArousalComponent>(User, out var arousalComponent))
            arousal = (byte) (arousalComponent.Arousal / 100 * 255);

        var availableActions = new HashSet<InteractionEntry>();
        foreach (var protoId in Target.Comp.ActionPrototypes)
        {
            if(!_prototypeManager.TryIndex(protoId,out var prototype))
                continue;

            var isAvailable = prototype.Checks.All(check => check.IsAvailable(User, Target, _entityManager));

            availableActions.Add(new InteractionEntry(protoId,isAvailable));
        }


        return new InteractionState(_entityManager.GetNetEntity(User), _entityManager.GetNetEntity(Target),availableActions,arousal);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case CloseEuiMessage:
                return;
            case InteractionSelectedMessage selectedMessage when Target.Comp.ActionPrototypes.Contains(selectedMessage.Id):
                _entityManager.System<InteractionPanelSystem>().Interact(User.Owner,Target.Owner,selectedMessage.Id);
                break;
        }

        StateDirty();
    }
}
