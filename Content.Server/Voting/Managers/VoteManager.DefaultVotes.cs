using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Presets;
using Content.Server.Maps;
using Content.Server.RoundEnd;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Voting;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Random;

namespace Content.Server.Voting.Managers
{
    public sealed partial class VoteManager
    {
        [ViewVariables]
        private string _previousMap = string.Empty;

        [ViewVariables]
        private readonly Dictionary<string, int> _presetDelay = new Dictionary<string, int>();

        private static readonly Dictionary<StandardVoteType, CVarDef<bool>> _voteTypesToEnableCVars = new()
        {
            {StandardVoteType.Restart, CCVars.VoteRestartEnabled},
            {StandardVoteType.Preset, CCVars.VotePresetEnabled},
            {StandardVoteType.Map, CCVars.VoteMapEnabled},
        };

        public void CreateStandardVote(IPlayerSession? initiator, StandardVoteType voteType)
        {
            if (initiator != null)
                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"{initiator} initiated a {voteType.ToString()} vote");
            else
                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Initiated a {voteType.ToString()} vote");

            switch (voteType)
            {
                case StandardVoteType.Restart:
                    CreateRestartVote(initiator);
                    break;
                case StandardVoteType.Preset:
                    CreatePresetVote(initiator);
                    break;
                case StandardVoteType.Map:
                    CreateMapVote(initiator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(voteType), voteType, null);
            }
            var ticker = _entityManager.EntitySysManager.GetEntitySystem<GameTicker>();
            ticker.UpdateInfoText();
            TimeoutStandardVote(voteType);
        }

        private void CreateRestartVote(IPlayerSession? initiator)
        {
            var alone = _playerManager.PlayerCount == 1 && initiator != null;
            var options = new VoteOptions
            {
                Title = Loc.GetString("ui-vote-restart-title"),
                Options =
                {
                    (Loc.GetString("ui-vote-restart-yes"), "yes"),
                    (Loc.GetString("ui-vote-restart-no"), "no"),
                    (Loc.GetString("ui-vote-restart-abstain"), "abstain")
                },
                Duration = alone
                    ? TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerAlone))
                    : TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerRestart)),
                InitiatorTimeout = TimeSpan.FromMinutes(5)
            };

            if (alone)
                options.InitiatorTimeout = TimeSpan.FromSeconds(10);

            WirePresetVoteInitiator(options, initiator);

            var vote = CreateVote(options);

            vote.OnFinished += (_, _) =>
            {
                var votesYes = vote.VotesPerOption["yes"];
                var votesNo = vote.VotesPerOption["no"];
                var total = votesYes + votesNo;

                var ratioRequired = _cfg.GetCVar(CCVars.VoteRestartRequiredRatio);
                if (total > 0 && votesYes / (float) total >= ratioRequired)
                {
                    _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Restart vote succeeded: {votesYes}/{votesNo}");
                    _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-restart-succeeded"));
                    var roundEnd = _entityManager.EntitySysManager.GetEntitySystem<RoundEndSystem>();
                    roundEnd.EndRound();
                }
                else
                {
                    _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Restart vote failed: {votesYes}/{votesNo}");
                    _chatManager.DispatchServerAnnouncement(
                        Loc.GetString("ui-vote-restart-failed", ("ratio", ratioRequired)));
                }
            };

            if (initiator != null)
            {
                // Cast yes vote if created the vote yourself.
                vote.CastVote(initiator, 0);
            }

            foreach (var player in _playerManager.ServerSessions)
            {
                if (player != initiator)
                {
                    // Everybody else defaults to an abstain vote to say they don't mind.
                    vote.CastVote(player, 2);
                }
            }
        }

        private void CreatePresetVote(IPlayerSession? initiator)
        {
            var presets = GetGamePresets();

            var alone = _playerManager.PlayerCount == 1 && initiator != null;
            var options = new VoteOptions
            {
                Title = Loc.GetString("ui-vote-gamemode-title"),
                Duration = alone
                    ? TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerAlone))
                    : TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerPreset))
            };

            if (alone)
                options.InitiatorTimeout = TimeSpan.FromSeconds(10);

            foreach (var (k, v) in presets)
            {
                options.Options.Add((Loc.GetString(v), k));
            }

            WirePresetVoteInitiator(options, initiator);

            var vote = CreateVote(options);

            vote.OnFinished += (_, args) =>
            {
                string picked;
                if (args.Winner == null)
                {
                    picked = (string) _random.Pick(args.Winners);
                    _chatManager.DispatchServerAnnouncement(
                        Loc.GetString("ui-vote-gamemode-tie", ("picked", Loc.GetString(presets[picked]))));
                }
                else
                {
                    picked = (string) args.Winner;
                    _chatManager.DispatchServerAnnouncement(
                        Loc.GetString("ui-vote-gamemode-win", ("winner", Loc.GetString(presets[picked]))));
                }

                if (_prototypeManager.TryIndex<GamePresetPrototype>(picked, out var prototype) && prototype.Delay > 0)
                    _presetDelay.Add(picked,prototype.Delay);


                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Preset vote finished: {picked}");
                var ticker = _entityManager.EntitySysManager.GetEntitySystem<GameTicker>();
                ticker.SetGamePreset(picked);
            };
        }

        private void CreateMapVote(IPlayerSession? initiator)
        {
            var maps = _gameMapManager.CurrentlyEligibleMaps().Where((prototype => prototype.MapName != _previousMap)).ToDictionary(map => map, map => map.MapName); //AMOUR MUR MUR

            var alone = _playerManager.PlayerCount == 1 && initiator != null;
            var options = new VoteOptions
            {
                Title = Loc.GetString("ui-vote-map-title"),
                Duration = alone
                    ? TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerAlone))
                    : TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerMap))
            };

            if (alone)
                options.InitiatorTimeout = TimeSpan.FromSeconds(10);

            foreach (var (k, v) in maps)
            {
                options.Options.Add((v, k));
            }

            WirePresetVoteInitiator(options, initiator);

            var vote = CreateVote(options);

            vote.OnFinished += (_, args) =>
            {
                GameMapPrototype picked;
                if (args.Winner == null)
                {
                    picked = (GameMapPrototype) _random.Pick(args.Winners);
                    _chatManager.DispatchServerAnnouncement(
                        Loc.GetString("ui-vote-map-tie", ("picked", maps[picked])));
                }
                else
                {
                    picked = (GameMapPrototype) args.Winner;
                    _chatManager.DispatchServerAnnouncement(
                        Loc.GetString("ui-vote-map-win", ("winner", maps[picked])));
                }

                _previousMap = picked.MapName; //AMOUR EDIT

                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Map vote finished: {picked.MapName}");
                var ticker = _entityManager.EntitySysManager.GetEntitySystem<GameTicker>();
                if (ticker.CanUpdateMap())
                {
                    if (_gameMapManager.TrySelectMapIfEligible(picked.ID))
                    {
                        ticker.UpdateInfoText();
                    }
                }
                else
                {
                    if (ticker.RoundPreloadTime <= TimeSpan.Zero)
                    {
                        _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-map-notlobby"));
                    }
                    else
                    {
                        var timeString = $"{ticker.RoundPreloadTime.Minutes:0}:{ticker.RoundPreloadTime.Seconds:00}";
                        _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-map-notlobby-time", ("time", timeString)));
                    }
                }
            };
        }

        private void TimeoutStandardVote(StandardVoteType type)
        {
            var timeout = TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteSameTypeTimeout));
            _standardVoteTimeout[type] = _timing.RealTime + timeout;
            DirtyCanCallVoteAll();
        }

        private Dictionary<string, string> GetGamePresets()
        {
            var presets = new Dictionary<string, string>();

            foreach (var preset in _prototypeManager.EnumeratePrototypes<GamePresetPrototype>())
            {
                if (_presetDelay.TryGetValue(preset.ID,out var delay)) //AMOUR EDIT
                {
                    var next = delay - 1;
                    if (next > 0)
                        _presetDelay[preset.ID] = next;
                    else
                        _presetDelay.Remove(preset.ID);

                    continue;
                }

                if(!preset.ShowInVote)
                    continue;

                if(_playerManager.PlayerCount < (preset.MinPlayers ?? int.MinValue))
                    continue;

                if(_playerManager.PlayerCount > (preset.MaxPlayers ?? int.MaxValue))
                    continue;

                presets[preset.ID] = preset.ModeTitle;
            }
            return presets;
        }
    }
}
