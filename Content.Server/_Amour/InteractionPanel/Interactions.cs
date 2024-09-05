using Content.Server._Amour.Animation;
using Content.Server._Amour.Hole;
using Content.Server._Honk.Cunt;
using Content.Server.Hands.Systems;
using Content.Server.Standing;
using Content.Shared._Amour.Hole;
using Content.Shared._Amour.InteractionPanel;
using Content.Shared._Honk.Cunt;
using Content.Shared.Hands.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Amour.InteractionPanel;

public sealed class Interactions : EntitySystem
{
    [Dependency] private readonly StandingStateSystem _standingState = default!;
    [Dependency] private readonly SharebleAnimationSystem _animationSystem = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly HoleSystem _holeSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly CuntSystem _cuntSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InteractionPanelComponent,InteractionBeginningEvent>(OnBegin);
        SubscribeLocalEvent<InteractionPanelComponent,InteractionEndingEvent>(OnEnd);
    }

    private void OnEnd(EntityUid uid, InteractionPanelComponent component, InteractionEndingEvent args)
    {
        if(args.Handled)
            return;

        switch (args.Id)
        {

        }
    }

    private void OnBegin(EntityUid uid, InteractionPanelComponent component, InteractionBeginningEvent args)
    {
        if(args.Handled)
            return;

        switch (args.Id)
        {
            case "PullTarget" :
                _pullingSystem.TryStartPull(uid, args.Target);
                break;
            case "CrawlTarget" :
                _standingState.TryLieDown(args.Target);
                break;
            case "ItemOnButt":
                PutHole(uid,"Anus");
                break;
            case "ItemOnVagina":
                PutHole(uid,"Vagina");
                break;
            case "ItemFromButt":
                TakeHole(uid,"Anus");
                break;
            case "ItemFromVagina":
                TakeHole(uid,"Vagina");
                break;
        }
    }

    private void PutHole(EntityUid uid,string holeType)
    {
        if (!TryComp<HandsComponent>(uid, out var handsComponent)
            || handsComponent.ActiveHand?.HeldEntity is null
            || !_holeSystem.TryFind(uid,holeType,out var hole))
            return;

        var ent = handsComponent.ActiveHand.HeldEntity.Value;

        _holeSystem.PutItem(ent,hole.Owner);
    }

    private void TakeHole(EntityUid uid,string holeType)
    {
        if (!_holeSystem.TryFind(uid,holeType,out var hole))
            return;

        foreach (var entity in _holeSystem.TakeItem(hole.Owner))
        {
            _handsSystem.TryPickup(uid, entity);
        }
    }
}
