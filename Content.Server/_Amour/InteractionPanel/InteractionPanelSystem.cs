using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.EUI;
using Content.Shared._Amour.InteractionPanel;
using Content.Shared._Amour.InteractionPanel.Checks;
using Content.Shared.ActionBlocker;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Amour.InteractionPanel;

public sealed class InteractionPanelSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InteractionPanelComponent, GetVerbsEvent<Verb>>(OnVerb);
        SubscribeLocalEvent<InteractionPanelComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<InteractionPanelComponent, PanelDoAfterEvent>(OnPanel);
        SubscribeLocalEvent<InteractionPanelComponent, ChangeDirectionAttemptEvent>(OnCancelable);
        SubscribeLocalEvent<InteractionPanelComponent, UpdateCanMoveEvent>(OnCancelable);
    }

    private void OnCancelable(EntityUid uid, InteractionPanelComponent component, CancellableEntityEventArgs args)
    {
        if (component.IsActive || component.IsBlocked)
        {
            args.Cancel();
        }
    }

    private void OnPanel(EntityUid uid, InteractionPanelComponent component, PanelDoAfterEvent args)
    {
        component.IsBlocked = false;
        if (args.Cancelled
           || !_prototypeManager.TryIndex<InteractionPrototype>(args.Prototype, out var prototype)
           || !TryComp<InteractionPanelComponent>(args.Target, out var targetInteractionPanelComponent))
            return;

        Interact(new Entity<InteractionPanelComponent>(uid, component), new Entity<InteractionPanelComponent>(args.Target.Value, targetInteractionPanelComponent), prototype, false);
    }

    private void OnInit(EntityUid uid, InteractionPanelComponent component, ComponentInit args)
    {
        component.Timeout = _gameTiming.CurTime;
        component.EndTime = _gameTiming.CurTime;

        if (_prototypeManager.TryIndex(component.ActionListPrototype, out var prototype))
        {
            component.ActionPrototypes.AddRange(prototype.Prototypes);
        }
    }

    private void OnVerb(EntityUid uid, InteractionPanelComponent component, GetVerbsEvent<Verb> args)
    {
        if (!_mobStateSystem.IsAlive(args.User) || !_mobStateSystem.IsAlive(uid))
            return;
        args.Verbs.Add(new Verb()
        {
            Text = Loc.GetString("interaction-open"),
            Act = () => OpenPanel(args.User, args.User, uid)
        });
    }

    public void OpenPanel(EntityUid panelOpener, Entity<InteractionPanelComponent?> user,
        Entity<InteractionPanelComponent?> target)
    {
        if (!Resolve(user, ref user.Comp) || !Resolve(target, ref target.Comp)
                                        || !_playerManager.TryGetSessionByEntity(panelOpener, out var session))
            return;

        if (!_mobStateSystem.IsAlive(user) || !_mobStateSystem.IsAlive(target))
            return;

        _eui.OpenEui(new InteractionPanelEui(
            new Entity<InteractionPanelComponent>(user, user.Comp),
            new Entity<InteractionPanelComponent>(target, target.Comp)),
            session);
    }

    public void Interact(Entity<InteractionPanelComponent?> user,
        Entity<InteractionPanelComponent?> target, ProtoId<InteractionPrototype> protoId)
    {
        //TODO: Пиздец... пиздец.... пиздец....
        if (!Resolve(user, ref user.Comp)
           || !Resolve(target, ref target.Comp)
           || user.Comp.IsActive || user.Comp.IsBlocked
           || target.Comp.IsActive || target.Comp.IsBlocked
           || user.Comp.Timeout > _gameTiming.CurTime
           || target.Comp.Timeout > _gameTiming.CurTime
           || !_prototypeManager.TryIndex(protoId, out var prototype))
            return;

        if (!_mobStateSystem.IsAlive(user) || !_mobStateSystem.IsAlive(target))
            return;

        if (!Check(user!, target!, prototype, out var check))
        {
            if (_playerManager.TryGetSessionByEntity(user, out var session) || session is null)
                return;

            var message = ParseMessage(target, $"interaction-fail-{check.GetType().Name.ToLower()}");
            _chatManager.ChatMessageToOne(ChatChannel.Emotes, message, message, EntityUid.Invalid, false, session.Channel);
            return;
        }

        if (prototype.BeginningTimeout == TimeSpan.Zero)
        {
            Interact(user!, target!, prototype);
            return;
        }

        user.Comp.IsBlocked = true;

        if (prototype.PreBeginMessages.Count > 0)
        {
            _chatSystem.TrySendInGameICMessage(user,
                ParseMessage(target, _robustRandom.Pick(prototype.PreBeginMessages)),
                InGameICChatType.Emote,
                false);
        }

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(
            EntityManager,
            user,
            prototype.BeginningTimeout,
            new PanelDoAfterEvent(prototype.ID), user, target
            )
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnHandChange = true
        });
    }

    private void Interact(Entity<InteractionPanelComponent> user,
        Entity<InteractionPanelComponent> target, InteractionPrototype prototype, bool hasChecked = true)
    {
        if (!hasChecked && !Check(user, target, prototype, out var check))
        {
            if (_playerManager.TryGetSessionByEntity(user, out var session) || session is null)
                return;

            var message = ParseMessage(target, $"interaction-fail-{check.GetType().Name.ToLower()}");
            _chatManager.ChatMessageToOne(ChatChannel.Emotes, message, message, EntityUid.Invalid, false, session.Channel);
            return;
        }


        user.Comp.Timeout = _gameTiming.CurTime + prototype.Timeout + prototype.EndTime;
        user.Comp.EndTime = _gameTiming.CurTime + prototype.EndTime;
        user.Comp.IsActive = true;
        user.Comp.CurrentAction = prototype.ID;
        user.Comp.CurrentPartner = new Entity<InteractionPanelComponent>(target, target.Comp);

        if (prototype.BeginningMessages.Count > 0)
        {
            _chatSystem.TrySendInGameICMessage(user,
                ParseMessage(target, _robustRandom.Pick(prototype.BeginningMessages)),
                InGameICChatType.Emote,
                false);
        }

        if (prototype.BeginningSound is not null)
            _audioSystem.PlayPvs(prototype.BeginningSound, user);

        foreach (var action in prototype.BeginningActions)
        {
            action.Run(user!, target!, EntityManager);
        }

        _actionBlockerSystem.UpdateCanMove(user);
        _actionBlockerSystem.UpdateCanMove(target);

        RaiseLocalEvent(user, new InteractionBeginningEvent(prototype.ID, user, target));
    }

    private string GetName(EntityUid target)
    {
        if (!TryComp<MindComponent>(target, out var mind) || mind.CharacterName is null)
            return MetaData(target).EntityName;

        return mind.CharacterName;
    }

    private Gender GetGender(EntityUid target)
    {
        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoidAppearanceComponent))
            return Gender.Male;

        return humanoidAppearanceComponent.Gender;
    }

    private string ParseMessage(EntityUid target, string message)
    {
        return Loc.GetString(message,
            ("target", GetName(target)), ("gender", GetGender(target)));
    }

    public bool Check(Entity<InteractionPanelComponent> user,
        Entity<InteractionPanelComponent> target, InteractionPrototype prototype, [NotNullWhen(false)] out IInteractionCheck? check)
    {
        check = null;
        foreach (var checkout in prototype.Checks.Where(check => !check.IsAvailable(user!, target!, EntityManager)))
        {
            check = checkout;
            return false;
        }

        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<InteractionPanelComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (component.EndTime > _gameTiming.CurTime || !component.IsActive)
                continue;

            if (component.CurrentPartner is null)
            {
                continue;
            }

            var user = new Entity<InteractionPanelComponent>(uid, component);
            var target = component.CurrentPartner.Value;

            if (_prototypeManager.TryIndex(component.CurrentAction, out var prototype))
            {
                if (prototype.EndingMessages.Count > 0)
                {
                    _chatSystem.TrySendInGameICMessage(uid,
                        ParseMessage(component.CurrentPartner.Value,
                            _robustRandom.Pick(prototype.EndingMessages)),
                        InGameICChatType.Emote,
                        false);
                }

                if (prototype.EndingSound is not null)
                    _audioSystem.PlayPvs(prototype.EndingSound, uid);

                foreach (var action in prototype.EndingActions)
                {
                    action.Run(user, component.CurrentPartner.Value, EntityManager);
                }
            }

            component.IsActive = false;

            _actionBlockerSystem.UpdateCanMove(user);
            _actionBlockerSystem.UpdateCanMove(target);

            RaiseLocalEvent(uid, new InteractionEndingEvent(component.CurrentAction,
                user,
                target));
        }
    }
}
