using System.Linq;
using Content.Server.Access.Systems;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Shared._Amour.Hole;
using Content.Shared._Amour.InteractionPanel;
using Content.Shared.Emoting;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Random.Helpers;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Server.Player;
using Robust.Shared.Audio;
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
    [Dependency] private readonly IdCardSystem _cardSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InteractionPanelComponent,GetVerbsEvent<Verb>>(OnVerb);
        SubscribeLocalEvent<InteractionPanelComponent,ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, InteractionPanelComponent component, ComponentInit args)
    {
        component.Timeout = _gameTiming.CurTime;
        component.EndTime = _gameTiming.CurTime;
    }

    private void OnVerb(EntityUid uid, InteractionPanelComponent component, GetVerbsEvent<Verb> args)
    {
        args.Verbs.Add(new Verb()
        {
            Text = "Open funny panel",
            Act = () => OpenPanel(args.User,args.User,uid)
        });
    }

    public void OpenPanel(EntityUid panelOpener, Entity<InteractionPanelComponent?> user,
        Entity<InteractionPanelComponent?> target)
    {
        if(!Resolve(user,ref user.Comp) || !Resolve(target,ref target.Comp)
                                        || !_playerManager.TryGetSessionByEntity(panelOpener, out var session))
            return;

        _eui.OpenEui(new InteractionPanelEui(
            new Entity<InteractionPanelComponent>(user,user.Comp),
            new Entity<InteractionPanelComponent>(target,target.Comp)),
            session);
    }

    public void Interact(Entity<InteractionPanelComponent?> user,
        Entity<InteractionPanelComponent?> target, ProtoId<InteractionPrototype> protoId)
    {
        //TODO: Пиздец... пиздец.... пиздец....
        if(   !Resolve(user,ref user.Comp)
           || !Resolve(target,ref target.Comp)
           || user.Comp.IsActive || user.Comp.IsBlocked
           || target.Comp.IsActive || target.Comp.IsBlocked
           || user.Comp.Timeout > _gameTiming.CurTime
           || target.Comp.Timeout > _gameTiming.CurTime
           || !_prototypeManager.TryIndex(protoId, out var prototype)
           || !prototype.Checks.All(check => check.IsAvailable(user!,target!,EntityManager)))
            return;

        user.Comp.Timeout = _gameTiming.CurTime + prototype.Timeout;
        user.Comp.EndTime = _gameTiming.CurTime + prototype.EndTime;
        user.Comp.IsActive = true;
        user.Comp.CurrentAction = protoId;
        user.Comp.CurrentPartner = new Entity<InteractionPanelComponent>(target,target.Comp);

        if(prototype.BeginningMessages.Count > 0)
        {
            _chatSystem.TrySendInGameICMessage(user,
                ParseMessage(target,_robustRandom.Pick(prototype.BeginningMessages)),
                InGameICChatType.Emote,
                false);
        }

        if (prototype.BeginningSound is not null)
            _audioSystem.PlayPvs(prototype.BeginningSound, user);

        RaiseLocalEvent(user,new InteractionBeginningEvent(protoId,user!,target!));
    }

    private string GetName(EntityUid target)
    {
        if(!TryComp<MindComponent>(target,out var mind) || mind.CharacterName is null)
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

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<InteractionPanelComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if(component.EndTime > _gameTiming.CurTime || !component.IsActive)
                continue;

            if (component.CurrentPartner is null)
            {
                continue;
            }

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
            }


            component.IsActive = false;
            RaiseLocalEvent(uid, new InteractionEndingEvent(component.CurrentAction,
                new Entity<InteractionPanelComponent>(uid,component),
                component.CurrentPartner.Value));
        }
    }
}
