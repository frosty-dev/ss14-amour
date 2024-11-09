﻿using System.Linq;
using Content.Server._Amour.StationEvents.Components;
using Content.Server.Chat.Systems;
using Content.Server.Fax;
using Content.Server.GameTicking.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.Fax.Components;
using Content.Shared.Paper;
using Robust.Shared.Random;
using Content.Server.RandomMetadata;
using Content.Server.Station.Systems;
using Content.Server.Station.Components;

namespace Content.Server._Amour.StationEvents
{
    public sealed class SpaceLawChangeRule : StationEventSystem<SpaceLawChangeRuleComponent>
    {
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly FaxSystem _faxSystem = default!;
        [Dependency] private readonly RandomMetadataSystem _randomMeta = default!;
        [Dependency] private readonly StationSystem _station = default!;
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
                "10"
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

            var station = "";
            foreach (var uniqueStation in _station.GetStationsSet())
            {
                if (HasComp<StationJobsComponent>(uniqueStation))
                {
                    station = MetaData(uniqueStation).EntityName;
                    break;
                }
            }

            var today = DateTime.Today.ToString("dd.MM");
            var namesList = new List<string>
            {
                "names_first_male",
                "names_last_male"
            };
            var operatorName = _randomMeta.GetRandomFromSegments(namesList, " ");

            var faxContent = Loc.GetString("station-event-space-law-change-form",
            ("station", station),
            ("date", today),
            ("operator", operatorName),
            ("text", message));

            var announcement = Loc.GetString("station-event-space-law-change-announcement-template",
            ("station", station),
            ("text", message)); ;

            _chat.DispatchGlobalAnnouncement(announcement, sender: Loc.GetString("admin-announce-announcer-default"), playSound: true, colorOverride: Color.Gold);

            SendSpaceLawChangeFax(faxContent);
        }

        /// <summary>
        ///     Sending a fax announcing changes in Space Law
        /// </summary>
        private void SendSpaceLawChangeFax(string message)
        {
            var printout = new FaxPrintout(
                message,
                Loc.GetString("materials-paper"),
                null,
                null,
                "paper_stamp-centcom",
                new List<StampDisplayInfo>
                {
                    new() { StampedName = Loc.GetString("stamp-component-stamped-name-centcom"), StampedColor = Color.FromHex("#006600") },
                });

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
