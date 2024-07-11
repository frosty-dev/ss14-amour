﻿using System.Linq;
using Content.Server._Amour.StationEvents.Components;
using Content.Server.Chat.Systems;
using Content.Server.Fax;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.Paper;
using Robust.Shared.Random;

namespace Content.Server._Amour.StationEvents
{
    public sealed class SpaceLawChangeRule : StationEventSystem<SpaceLawChangeRuleComponent>
    {
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly FaxSystem _faxSystem = default!;
        private readonly RandomSelector _randomSelector = default!;

        public SpaceLawChangeRule()
        {
            var options = new List<string>
            {
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
                "11"
                // Add other message options here if necessary
            };

            _randomSelector = new RandomSelector(options);
        }

        protected override void Started(EntityUid uid, SpaceLawChangeRuleComponent component,
            GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, component, gameRule, args);

            var randomMessage = _randomSelector.GetRandom(_robustRandom);

            var message = Loc.GetString($"station-event-space-law-change-announcement-{randomMessage}");

            _chat.DispatchGlobalAnnouncement(message, playSound: true, colorOverride: Color.Gold);

            SendSpaceLawChangeFax(message);
        }

        /// <summary>
        ///     Sending a fax announcing changes in Space Law
        /// </summary>
        private void SendSpaceLawChangeFax(string message)
        {
            var printout = new FaxPrintout(
                message,
                Loc.GetString("materials-paper"),
                stampedBy: new List<StampDisplayInfo>
                {
                    new() { StampedName = Loc.GetString("stamp-component-stamped-name-centcom"), StampedColor = Color.FromHex("#006600") }
                },
                prototypeId: null!,
                stampState: null);

            var faxes = EntityManager.EntityQuery<FaxMachineComponent>();
            foreach (var fax in faxes)
            {
                _faxSystem.Receive(fax.Owner, printout, null, fax);
            }
        }
    }

    /// <summary>
    ///     Randomly selects an option from the list. Selected options cannot be selected again until all unselected options are exhausted.
    /// </summary>
    public class RandomSelector
    {
        private readonly List<string> _options;
        private readonly List<string> _exclusions;

        public RandomSelector(List<string> options)
        {
            _options = options;
            _exclusions = new List<string>();
        }

        public string GetRandom(IRobustRandom robustRandom)
        {
            if (_exclusions.Count >= _options.Count)
                _exclusions.Clear();

            var availableOptions = _options.Except(_exclusions).ToList();
            if (availableOptions.Count == 0)
            {
                _exclusions.Clear();
                availableOptions = _options.ToList();
            }

            var selectedOption = robustRandom.PickAndTake(availableOptions);
            _exclusions.Add(selectedOption);

            return selectedOption;
        }
    }

}
