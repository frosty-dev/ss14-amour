using System.Linq;
using Content.Server.EUI;
using Content.Server.Interaction;
using Content.Shared._Amour.Arousal;
using Content.Shared._Amour.InteractionPanel;
using Content.Shared._Amour.InteractionPanel.Checks;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;
using Robust.Shared.Reflection;

namespace Content.Server._Amour.InteractionPanel;

public sealed class InteractionPanelEui : BaseEui
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IReflectionManager _reflectionManager = default!;
    private readonly InteractionPanelSystem _interactionPanelSystem;

    public readonly Entity<InteractionPanelComponent> User;
    public readonly Entity<InteractionPanelComponent> Target;

    public readonly List<IInteractionCheck> UserInfo = new()
    {
        new UserHasBreast(),
        new UserHasButt(),
        new UserHasPenis(),
        new UserHasTesticles(),
        new UserHasVagina(),
        new IsUserCrawl()
    };

    public readonly List<IInteractionCheck> TargetInfo = new()
    {
        new HasSmallDistance(),
        new TargetHasBreast(),
        new TargetHasButt(),
        new TargetHasPenis(),
        new TargetHasTesticles(),
        new TargetHasVagina(),
        new IsTargetCrawl()
    };

    public InteractionPanelEui(Entity<InteractionPanelComponent> user, Entity<InteractionPanelComponent> target)
    {
        IoCManager.InjectDependencies(this);

        _interactionPanelSystem = _entityManager.System<InteractionPanelSystem>();
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

            var isAvailable = _interactionPanelSystem.Check(User, Target, prototype, out _);

            availableActions.Add(new InteractionEntry(protoId,isAvailable));
        }

        var descUser = new HashSet<string>();
        foreach (var check in UserInfo)
        {
            if (TryCheck(check, out var desc))
                descUser.Add(desc);
        }

        var descTarget = new HashSet<string>();
        foreach (var check in TargetInfo)
        {
            if (TryCheck(check, out var desc))
                descTarget.Add(desc);
        }

        return new InteractionState(_entityManager.GetNetEntity(User), _entityManager.GetNetEntity(Target),availableActions,arousal, descUser, descTarget);
    }

    private bool TryCheck(IInteractionCheck check, out string output)
    {
        output = $"interaction-success-{check.GetType().Name.ToLower()}";
        return check.IsAvailable(User, Target, _entityManager);
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
