using FootballManager.Application.Helpers;
using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Interfaces.Repositories.EFC;
using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Dtos;
using FootballManager.Application.Models.Requests;
using FootballManager.Application.Models.Responses;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FootballManager.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ISeasonRepository _seasonRepository;
        private readonly ISeasonSettingsRepository _seasonSettingsRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IPlayerEfcRepository _playerEfcRepository;
        private readonly IOptions<ScheduleWeeklyLeagueSettings> _scheduleWeeklyLeagueSettings;
        private readonly IOptions<ScheduleWeeklyTournamentSettings> _scheduleWeeklyTournamentSettings;
        private readonly ILogger<JobService> _logger;
        private readonly ISeasonService _seasonService; // TODO DELETE

        public JobService(IMatchRepository matchRepository, ISeasonRepository seasonRepository, ISeasonSettingsRepository seasonSettingsRepository, IUserRepository userRepository, ITeamRepository teamRepository, IPlayerRepository playerRepository, IPlayerEfcRepository playerEfcRepository, IOptions<ScheduleWeeklyLeagueSettings> scheduleWeeklyLeagueSettings, IOptions<ScheduleWeeklyTournamentSettings> scheduleWeeklyTournamentSettings, ILogger<JobService> logger, ISeasonService seasonService)
        {
            _matchRepository = matchRepository ?? throw new ArgumentNullException(nameof(matchRepository));
            _seasonRepository = seasonRepository ?? throw new ArgumentNullException(nameof(seasonRepository));
            _seasonSettingsRepository = seasonSettingsRepository ?? throw new ArgumentNullException(nameof(seasonSettingsRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
            _playerEfcRepository = playerEfcRepository ?? throw new ArgumentNullException(nameof(playerEfcRepository));
            _scheduleWeeklyLeagueSettings = scheduleWeeklyLeagueSettings ?? throw new ArgumentNullException(nameof(scheduleWeeklyLeagueSettings));
            _scheduleWeeklyTournamentSettings = scheduleWeeklyTournamentSettings ?? throw new ArgumentNullException(nameof(scheduleWeeklyTournamentSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _seasonService = seasonService;
        }


        public async Task SimulateMatchesJob()
        {
            var matches = await _matchRepository.GetMatchesToSimulate();

            foreach (var match in matches)
            {
                BackgroundJob.Enqueue<IMatchService>(matchservice => matchservice.SimulateMatch(match.Id));
            }
        }

        public async Task ConcludeSeasons()
        {
            BackgroundJob.Enqueue<IJobService>(jobService => jobService.ConcludeLeagues());
            BackgroundJob.Enqueue<IJobService>(jobService => jobService.ProceedTournaments());
        }

        public async Task ConcludeLeagues()
        {
            var leaguesToConclude = await _seasonRepository.GetLeaguesToConclude();

            foreach (var leagueToConclude in leaguesToConclude)
                BackgroundJob.Enqueue<ISeasonService>(seasonservice => seasonservice.ConcludeLeague(leagueToConclude.SeasonId));
        }

        public async Task ProceedTournaments()
        {
            var tournamentsToProceed = await _seasonRepository.CheckTournamentsToProceed();

            foreach (var tournamentToProceed in tournamentsToProceed)
                await _seasonService.ProceedTournament(tournamentToProceed.SeasonId);
            //BackgroundJob.Enqueue<ISeasonService>(seasonservice => seasonservice.ProceedTournament(tournamentToProceed.SeasonId));
        }

        public async Task ScheduleWeeklyLeague()
        {
            var premadeTeams = PreMadeTeams.Teams.Where(team => team.PremadeTeamType == PremadeTeamType.Club).ToList();
            int? numberOfTeamsToParticipate = _scheduleWeeklyLeagueSettings.Value.NumberOfTeamsToParticipate;
            Random _random = new();

            if (premadeTeams == null || numberOfTeamsToParticipate == null || premadeTeams.Count < numberOfTeamsToParticipate)
                return;

            var lastSeason = await _seasonRepository.GetLastWeeklyLeague();

            if (lastSeason != null && lastSeason.ChampionId == null)
                return;

            int lastSeasonNumber = Int32.Parse(lastSeason != null ? lastSeason.Name.Split(' ')[0].Replace(".", "") : "0");
            string seasonNumber = (lastSeasonNumber + 1).ToString() + ". ";

            string seasonName = seasonNumber + _scheduleWeeklyLeagueSettings.Value.SeasonName + " (WL)";
            var season = await _seasonRepository.CreateSeason(new CreateSeasonRequest() { Name = seasonName, SeasonType = SeasonType.League });
            try
            {
                var newSeasonSettings = await _seasonSettingsRepository.CreateSeasonSettings(new SeasonSettingsCommand() { SeasonId = season.Id, AllowNoAssistGoals = _scheduleWeeklyLeagueSettings.Value.AllowNoAssistGoals });
            }
            catch (Exception ex)
            {
                _logger.LogError($"JobService.ScheduleWeeklyLeague.CreateSeasonSettings ErrorMessage: '{ex.Message}'");
                await _seasonSettingsRepository.DeleteSeasonSettingsBySeasonId(season.Id);
                await _seasonRepository.DeleteSeasonById(season.Id);
            }

            var dummyManager = await _userRepository.GetFirstUser();

            if (dummyManager == null)
            {
                // Moved initializing first User to Program.cs / InitializeMsSqlDb / DatabaseSeeder
                return;
            }

            int teamsCreated = 0;

            try
            {
                while (teamsCreated < numberOfTeamsToParticipate)
                {
                    TeamCommand newTeamCommand = new();
                    Team newTeam = new();
                    List<BulkInsertPlayer> newPlayers = new();
                    PremadeTeam premadeTeam = premadeTeams[_random.Next(0, premadeTeams.Count)];

                    newTeamCommand.Name = premadeTeam.Name;
                    newTeamCommand.ManagerId = dummyManager.Id;
                    newTeamCommand.SeasonId = season.Id;

                    try { newTeam = await _teamRepository.CreateTeam(newTeamCommand); }
                    catch (Exception ex)
                    {
                        _logger.LogError($"JobService.ScheduleWeeklyLeague.CreateTeam Error: Failed to Create Team. TeamCommand: '{JsonConvert.SerializeObject(newTeamCommand)}'. ErrorMessage: '{ex.Message}'");
                        throw;
                    }

                    foreach (var player in premadeTeam.Players)
                    {
                        BulkInsertPlayer newPlayer = new();
                        newPlayer.Name = player.Name;
                        newPlayer.Position = player.Position;
                        newPlayer.Number = player.Number;
                        newPlayer.TeamId = newTeam.Id;
                        newPlayer.SeasonId = season.Id;
                        newPlayers.Add(newPlayer);
                    }

                    try
                    {
                        await _playerEfcRepository.BulkInsertPlayersForTeam(newPlayers);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"JobService.ScheduleWeeklyLeague.BulkInsertPlayers Error: Failed to BulkInsert Players. Players: '{JsonConvert.SerializeObject(newPlayers)}'. ErrorMessage: '{ex.Message}'.", 500);
                        throw;
                    }
                    teamsCreated++;
                    premadeTeams.Remove(premadeTeam);
                }
            }
            catch
            {
                await _playerRepository.DeletePlayersBySeasonId(season.Id);
                await _teamRepository.DeleteTeamsBySeasonId(season.Id);
                await _seasonSettingsRepository.DeleteSeasonSettingsBySeasonId(season.Id);
                await _seasonRepository.DeleteSeasonById(season.Id);
                return;
            }

            ScheduleSeasonRequest scheduleSeasonRequest = new();
            scheduleSeasonRequest.SeasonId = season.Id;
            scheduleSeasonRequest.SeasonType = Domain.Enums.SeasonType.League;
            scheduleSeasonRequest.DailyMatchHours = _scheduleWeeklyLeagueSettings.Value.MatchTimes;
            scheduleSeasonRequest.MatchIntervalMinutes = _scheduleWeeklyLeagueSettings.Value.MatchIntervalMinutes;
            scheduleSeasonRequest.SeasonHalvesMinimumIntervalHours = _scheduleWeeklyLeagueSettings.Value.LeagueHalvesMinumumIntervalHours;

            DateTime firstMatchAfter = new DateTime(DateTime.UtcNow.AddDays(1).Year, DateTime.UtcNow.AddDays(1).Month, DateTime.UtcNow.AddDays(1).Day, 0, 0, 0); // Earliest Time : Today-Tomorrow Midnight 

            if (_scheduleWeeklyLeagueSettings.Value.SeasonStartDay != null)
            {
                if ((int)firstMatchAfter.DayOfWeek != _scheduleWeeklyLeagueSettings.Value.SeasonStartDay)
                {
                    firstMatchAfter = new DateTime(firstMatchAfter.Year, firstMatchAfter.Month, firstMatchAfter.Day, 0, 0, 0);

                    while ((int)firstMatchAfter.DayOfWeek != _scheduleWeeklyLeagueSettings.Value.SeasonStartDay)
                        firstMatchAfter = firstMatchAfter.AddDays(1);
                }
                scheduleSeasonRequest.FirstMatchAfter = firstMatchAfter;
            }

            BackgroundJob.Enqueue<ISeasonService>(seasonservice => seasonservice.ScheduleSeason(scheduleSeasonRequest));
        }

        public async Task ScheduleWeeklyTournament()
        {
            var premadeTeams = PreMadeTeams.Teams.Where(team => team.PremadeTeamType == PremadeTeamType.National).ToList();
            int? numberOfTeamsToParticipate = _scheduleWeeklyTournamentSettings.Value.NumberOfTeamsToParticipate;
            Random _random = new();

            if (premadeTeams == null || numberOfTeamsToParticipate == null || premadeTeams.Count < numberOfTeamsToParticipate)
                return;

            // Tournament Format - Teams of Power of 2

            var logOfTwo = Math.Log((int)numberOfTeamsToParticipate, 2);
            if (logOfTwo % 1 != 0)
                return;

            var lastSeason = await _seasonRepository.GetLastWeeklyTournament();

            if (lastSeason != null && lastSeason.ChampionId == null)
                return;

            string seasonNumber = "1. ";

            if (lastSeason != null)
            {
                int lastSeasonNumber = Int32.Parse(lastSeason.Name.Split(' ')[0].Replace(".", ""));
                seasonNumber = (lastSeasonNumber + 1).ToString() + ". ";
            }

            string seasonName = seasonNumber + _scheduleWeeklyLeagueSettings.Value.SeasonName + " (WT)";
            var season = await _seasonRepository.CreateSeason(new CreateSeasonRequest() { Name = seasonName, SeasonType = SeasonType.Tournament });

            var dummyManager = await _userRepository.GetFirstUser();

            if (dummyManager == null) // Moved initializing first User to Program.cs / InitializeMsSqlDb / DatabaseSeeder
                return;

            int teamsCreated = 0;

            try
            {
                while (teamsCreated < numberOfTeamsToParticipate)
                {
                    TeamCommand newTeamCommand = new();
                    Team newTeam = new();
                    List<BulkInsertPlayer> newPlayers = new();
                    PremadeTeam premadeTeam = premadeTeams[_random.Next(0, premadeTeams.Count)];

                    newTeamCommand.Name = premadeTeam.Name;
                    newTeamCommand.ManagerId = dummyManager.Id;
                    newTeamCommand.SeasonId = season.Id;

                    try { newTeam = await _teamRepository.CreateTeam(newTeamCommand); }
                    catch (Exception ex)
                    {
                        _logger.LogError($"JobService.ScheduleWeeklyLeague.CreateTeam Error: Failed to Create Team. TeamCommand: '{JsonConvert.SerializeObject(newTeamCommand)}'. ErrorMessage: '{ex.Message}'");
                        throw;
                    }

                    foreach (var player in premadeTeam.Players)
                    {
                        BulkInsertPlayer newPlayer = new();
                        newPlayer.Name = player.Name;
                        newPlayer.Position = player.Position;
                        newPlayer.Number = player.Number;
                        newPlayer.TeamId = newTeam.Id;
                        newPlayer.SeasonId = season.Id;
                        newPlayers.Add(newPlayer);
                    }

                    try { await _playerEfcRepository.BulkInsertPlayersForTeam(newPlayers); }
                    catch (Exception ex)
                    {
                        _logger.LogError($"JobService.ScheduleWeeklyLeague.BulkInsertPlayers Error: Failed to BulkInsert Players. Players: '{JsonConvert.SerializeObject(newPlayers)}'. ErrorMessage: '{ex.Message}'.", 500);
                        throw;
                    }
                    teamsCreated++;
                    premadeTeams.Remove(premadeTeam);
                }
            }
            catch
            {
                await _playerRepository.DeletePlayersBySeasonId(season.Id);
                await _teamRepository.DeleteTeamsBySeasonId(season.Id);
                await _seasonRepository.DeleteSeasonById(season.Id);
                return;
            }

            ScheduleSeasonRequest scheduleSeasonRequest = new();
            scheduleSeasonRequest.SeasonId = season.Id;
            scheduleSeasonRequest.SeasonType = Domain.Enums.SeasonType.Tournament;
            scheduleSeasonRequest.DailyMatchHours = _scheduleWeeklyTournamentSettings.Value.MatchTimes;
            scheduleSeasonRequest.MatchIntervalMinutes = _scheduleWeeklyTournamentSettings.Value.MatchIntervalMinutes;

            DateTime firstMatchAfter = DateTime.UtcNow;

            if (_scheduleWeeklyTournamentSettings.Value.SeasonStartDay != null)
            {
                if ((int)firstMatchAfter.DayOfWeek != _scheduleWeeklyTournamentSettings.Value.SeasonStartDay)
                {
                    firstMatchAfter = new DateTime(firstMatchAfter.Year, firstMatchAfter.Month, firstMatchAfter.Day, 0, 0, 0);

                    while ((int)firstMatchAfter.DayOfWeek != _scheduleWeeklyTournamentSettings.Value.SeasonStartDay)
                        firstMatchAfter = firstMatchAfter.AddDays(1);
                }
                scheduleSeasonRequest.FirstMatchAfter = firstMatchAfter;
            }

            BackgroundJob.Enqueue<ISeasonService>(seasonservice => seasonservice.ScheduleSeason(scheduleSeasonRequest));
        }
    }
}
