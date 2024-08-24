using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server._White.AspectsSystem.Aspects.Components;
using Content.Server._White.AspectsSystem.Base;
using Content.Server.GameTicking.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server._White.AspectsSystem.Aspects
{
    public sealed class TraitoredAspect : AspectSystem<TraitoredAspectComponent>
    {
        [Dependency] private readonly TraitorRuleSystem _traitorRuleSystem = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        private bool _announcedForTraitors;

        private float _timeElapsed;
        private float _timeElapsedForTraitor;

        private float _wacky;
        private const float WackyAaa = 60;

        protected override void Started(EntityUid uid, TraitoredAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, component, gameRule, args);

            // Just to make sure
            ResetValues();

            if (!HasTraitorGameRule())
                ForceEndSelf(uid, gameRule);

            _wacky = _random.Next(300, 360);
        }

        protected override void ActiveTick(EntityUid uid, TraitoredAspectComponent component, GameRuleComponent gameRule, float frameTime)
        {
            base.ActiveTick(uid, component, gameRule, frameTime);

            _timeElapsedForTraitor += frameTime;
            _timeElapsed += frameTime;

            if (_timeElapsedForTraitor >= WackyAaa && !_announcedForTraitors)
            {
                AnnounceToTators(uid, gameRule);
                _announcedForTraitors = true;
            }

            if (_timeElapsed >= _wacky)
            {
                AnnounceToAll(uid, gameRule);
            }
        }

        protected override void Ended(EntityUid uid, TraitoredAspectComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
        {
            base.Ended(uid, component, gameRule, args);
            ResetValues();
        }

        #region Helpers

        private void AnnounceToTators(EntityUid uid, GameRuleComponent rule)
        {
            var traitors = _traitorRuleSystem.GetOtherTraitorMindsAliveAndConnected(null);

            if (traitors.Count == 0)
            {
                ForceEndSelf(uid, rule);
            }

            foreach (var traitor in traitors)
            {
                if (!_mindSystem.TryGetSession(traitor.Mind, out var session))
                    continue;

                var traitorMind = traitor.Mind.OwnedEntity;

                if (traitorMind == null)
                    return;

                _chatManager.DispatchServerMessage(session, "Внимание, коммуникации синдиката перехвачены, вас раскрыли!");
                _audio.PlayEntity("/Audio/White/Aspects/palevo.ogg", traitorMind.Value, traitorMind.Value);
            }
        }

        private void AnnounceToAll(EntityUid uid, GameRuleComponent rule)
        {
            var traitors = _traitorRuleSystem.GetOtherTraitorMindsAliveAndConnected(null);

            var msg = "Станция, служба контрразведки нанотрейзен рассекретила секретную передачу Синдиката и выяснила имена проникниших на вашу станцию агентов. Агенты имеют следующие имена: \n";

            foreach (var traitor in traitors)
            {
                var name = traitor.Mind.CharacterName;
                if (!string.IsNullOrEmpty(name))
                {
                    msg += $" {name} - УБЕЙТЕ ЕГО НАХУЙ\n";
                }
            }

            _chatSystem.DispatchGlobalAnnouncement(msg, "Мяукиман Крысус", colorOverride: Color.Aquamarine);

            ForceEndSelf(uid, rule);
        }

        private void ResetValues()
        {
            _announcedForTraitors = false;
            _timeElapsed = 0;
            _timeElapsedForTraitor = 0;
        }

        private bool HasTraitorGameRule()
        {
            return EntityQuery<TraitorRuleComponent>().Any();
        }

        #endregion

    }
}
