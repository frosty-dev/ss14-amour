using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Speech.Components;
using Content.Server._White.AspectsSystem.Aspects.Components;
using Content.Server._White.AspectsSystem.Base;
using Content.Server.GameTicking.Components;
using Content.Shared.Mind.Components;
using Robust.Shared.Random;

namespace Content.Server._White.AspectsSystem.Aspects;

public sealed class RandomAccentAspect : AspectSystem<RandomAccentAspectComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLateJoin);
    }

    protected override void Started(EntityUid uid, RandomAccentAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        var query = EntityQueryEnumerator<MindContainerComponent>();

        while (query.MoveNext(out var ent, out _))
        {
            ApplyRandomAccent(ent);
            ChatHelper.SendAspectDescription(ent, Loc.GetString("random-accent-aspect-desc"));
        }
    }

    private void HandleLateJoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<RandomAccentAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            if (!ev.LateJoin)
                return;

            var mob = ev.Mob;
            ChatHelper.SendAspectDescription(mob, Loc.GetString("random-accent-aspect-desc"));
        }
    }

    #region Helpers


    // TODO: Move this to prototypes.
    private enum AccentType
    {
        Stuttering,
        Spanish,
        Slurred,
        Scrambled,
        Pirate,
        Russian,
        OwO,
        Lizard,
        Backwards,
        Bark,
        Anxiety,
        Moth,
        French,
        Gnome,
    }

    private void ApplyRandomAccent(EntityUid uid)
    {
        var allAccents = Enum.GetValues(typeof(AccentType)).Cast<AccentType>().ToList();

        var randomIndex = _random.Next(allAccents.Count);
        var selectedAccent = allAccents[randomIndex];

        ApplyAccent(uid, selectedAccent);
    }

    private void ApplyAccent(EntityUid uid, AccentType accentType)
    {
        switch (accentType)
        {
            case AccentType.Stuttering:
                EntityManager.EnsureComponent<StutteringAccentComponent>(uid);
                break;
            case AccentType.Spanish:
                EntityManager.EnsureComponent<SpanishAccentComponent>(uid);
                break;
            case AccentType.Slurred:
                EntityManager.EnsureComponent<SlurredAccentComponent>(uid);
                break;
            case AccentType.Scrambled:
                EntityManager.EnsureComponent<ScrambledAccentComponent>(uid);
                break;
            case AccentType.Pirate:
                EntityManager.EnsureComponent<PirateAccentComponent>(uid);
                break;
            case AccentType.Russian:
                EntityManager.EnsureComponent<RussianAccentComponent>(uid);
                break;
            case AccentType.OwO:
                EntityManager.EnsureComponent<OwOAccentComponent>(uid);
                break;
            case AccentType.Lizard:
                EntityManager.EnsureComponent<LizardAccentComponent>(uid);
                break;
            /* Not funny
            case AccentType.Backwards:
                EntityManager.EnsureComponent<BackwardsAccentComponent>(uid);
                break;
            */
            case AccentType.Bark:
                EntityManager.EnsureComponent<BarkAccentComponent>(uid);
                break;
            case AccentType.Anxiety:
                var stutter = EntityManager.EnsureComponent<StutteringAccentComponent>(uid);
                stutter.MatchRandomProb = 0.2f;
                stutter.FourRandomProb = 0f;
                stutter.ThreeRandomProb = 0.3f;
                stutter.CutRandomProb = 0f;
                break;
            case AccentType.Moth:
                EntityManager.EnsureComponent<MothAccentComponent>(uid);
                break;
            case AccentType.French:
                EntityManager.EnsureComponent<FrenchAccentComponent>(uid);
                break;
            case AccentType.Gnome:
                EntityManager.EnsureComponent<GnomeAccentComponent>(uid);
                break;
        }
    }

    #endregion
}
