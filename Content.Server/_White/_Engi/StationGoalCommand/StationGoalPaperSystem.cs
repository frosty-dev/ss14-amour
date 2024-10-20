using Content.Server.Fax;
using Content.Server.GameTicking.Events;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Fax.Components;
using Content.Shared.Paper;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.RandomMetadata;

namespace Content.Server._White._Engi.StationGoal
{
    /// <summary>
    /// WD ENGI EXCLUSIVE.
    /// System to spawn paper with station goal.
    /// </summary>
    public sealed class StationGoalPaperSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly FaxSystem _fax = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly RandomMetadataSystem _randomMeta = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        }

        private void OnRoundStarting(RoundStartingEvent ev)
        {
            var playerCount = _playerManager.PlayerCount;

            var query = EntityQueryEnumerator<StationGoalComponent>();
            while (query.MoveNext(out var uid, out var station))
            {
                var tempGoals = new List<ProtoId<StationGoalPrototype>>(station.Goals);
                StationGoalPrototype? selGoal = null;
                while (tempGoals.Count > 0)
                {
                    var goalId = _random.Pick(tempGoals);
                    var goalProto = _proto.Index(goalId);

                    if (playerCount > goalProto.MaxPlayers ||
                        playerCount < goalProto.MinPlayers)
                    {
                        tempGoals.Remove(goalId);
                        continue;
                    }

                    selGoal = goalProto;
                    break;
                }

                if (selGoal is null)
                    return;

                if (SendStationGoal(uid, selGoal))
                {
                    Log.Info($"Goal {selGoal.ID} has been sent to station {MetaData(uid).EntityName}");
                }
            }
        }

        public bool SendStationGoal(EntityUid? ent, ProtoId<StationGoalPrototype> goal)
        {
            return SendStationGoal(ent, _proto.Index(goal));
        }

        /// <summary>
        /// WD ENGI EXCLUSIVE.
        /// Send a station goal on selected station to all faxes which are authorized to receive it.
        /// </summary>
        /// <returns>True if at least one fax received paper</returns>
        public bool SendStationGoal(EntityUid? ent, StationGoalPrototype goal)
        {
            if (ent is null)
                return false;

            if (!TryComp<StationDataComponent>(ent, out var stationData))
                return false;

            var today = DateTime.Today.ToString("dd.MM");
            var namesList = new List<string>
            {
                "names_first_male",
                "names_last_male"
            };
            var operatorName = _randomMeta.GetRandomFromSegments(namesList, " ");

            var faxContent = Loc.GetString("engi-station-goal-form",
            ("station", MetaData(ent.Value).EntityName),
            ("date", today),
            ("operator", operatorName),
            ("text", Loc.GetString(goal.Text)));

            var printout = new FaxPrintout(
                faxContent,
                Loc.GetString("engi-station-goal-fax-paper-name"),
                null,
                null,
                "paper_stamp-centcom",
                new List<StampDisplayInfo>
                {
                    new() { StampedName = Loc.GetString("stamp-component-stamped-name-centcom"), StampedColor = Color.FromHex("#006600") },
                });

            var wasSent = false;
            var query = EntityQueryEnumerator<FaxMachineComponent>();
            while (query.MoveNext(out var faxUid, out var fax))
            {
                if (!fax.ReceiveStationGoal)
                    continue;

                var largestGrid = _station.GetLargestGrid(stationData);
                var grid = Transform(faxUid).GridUid;
                if (grid is not null && largestGrid == grid.Value)
                {
                    _fax.Receive(faxUid, printout, null, fax);
                    foreach (var spawnEnt in goal.Spawns)
                    {
                        SpawnAtPosition(spawnEnt, Transform(faxUid).Coordinates);
                    }
                    wasSent = true;
                }
            }
            return wasSent;
        }
    }
}
