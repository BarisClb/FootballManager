using FootballManager.Application.Helpers;
using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Interfaces.Repositories.EFC;
using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Dtos;
using FootballManager.Application.Models.Requests;
using FootballManager.Application.Models.Responses;
using FootballManager.Application.Models.View;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FootballManager.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IPlayerEfcRepository _playerEfcRepository;
        private readonly ISeasonPassRepository _seasonPassRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISeasonRepository _seasonRepository;
        private readonly IOptions<SeasonsSettings> _seasonsSettings;
        private readonly ILogger<TeamService> _logger;

        public TeamService(ITeamRepository teamRepository, IPlayerEfcRepository playerEfcRepository, ISeasonPassRepository seasonPassRepository, IUserRepository userRepository, ISeasonRepository seasonRepository, IOptions<SeasonsSettings> seasonsSettings, ILogger<TeamService> logger)
        {
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _playerEfcRepository = playerEfcRepository ?? throw new ArgumentNullException(nameof(playerEfcRepository));
            _seasonPassRepository = seasonPassRepository ?? throw new ArgumentNullException(nameof(seasonPassRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _seasonRepository = seasonRepository ?? throw new ArgumentNullException(nameof(seasonRepository));
            _seasonsSettings = seasonsSettings ?? throw new ArgumentNullException(nameof(seasonsSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<BaseResponse<Team>> CreateTeam(CreateTeamRequest createTeamRequest)
        {
            // Check User
            if (string.IsNullOrEmpty(createTeamRequest.UserUsername) || string.IsNullOrEmpty(createTeamRequest.UserPassword))
                return BaseResponse<Team>.Fail($"No Username or Password provided. Username: '{createTeamRequest.UserUsername}'.", 400);
            var user = await _userRepository.GetUserByUsernameAndPassword(createTeamRequest.UserUsername, createTeamRequest.UserPassword);
            if (user == null)
                return BaseResponse<Team>.Fail($"Username or Password is wrong. Username: '{createTeamRequest.UserUsername}'.", 400);

            // Default SeasonId if empty
            createTeamRequest.SeasonId ??= _seasonsSettings.Value.CurrentSeasonId ?? await _seasonRepository.GetLastSeasonId();

            // Check Season
            var season = await _seasonRepository.GetSeasonById(createTeamRequest.SeasonId ?? 0);
            if (season == null)
                return BaseResponse<Team>.Fail($"Season not found. SeasonId: '{createTeamRequest.SeasonId}'.", 400);
            else if (!season.IsRegistrationOpen)
                return BaseResponse<Team>.Fail($"Registration is closed for Season. SeasonId: '{createTeamRequest.SeasonId}'.", 400);

            // Check SeasonPass
            var seasonPass = await _seasonPassRepository.GetValidSeasonPassBySeasonIdAndPassword(createTeamRequest.SeasonId ?? 0, createTeamRequest.SeasonPassPassword ?? "");
            if (seasonPass == null)
                return BaseResponse<Team>.Fail($"SeasonPass not found. SeasonId: '{createTeamRequest.SeasonId}', SeasonPassPassword: '{createTeamRequest.SeasonPassPassword}'.", 400);

            // Check Team
            var team = await _teamRepository.FindTeamBySeasonAndManager(createTeamRequest.SeasonId ?? 0, user.Id);
            if (team != null)
                return BaseResponse<Team>.Fail($"Manager already has a Team this Season. ManagerUsername: '{createTeamRequest.UserUsername}', SeasonId: '{createTeamRequest.SeasonId}'.", 400);
            if (string.IsNullOrEmpty(createTeamRequest.TeamName))
                return BaseResponse<Team>.Fail($"No Team Name provided.", 400);
            if (createTeamRequest.Players == null || createTeamRequest.Players.Count < 11)
                return BaseResponse<Team>.Fail($"Team Members are not valid. Players: '{JsonConvert.SerializeObject(createTeamRequest.Players)}'.", 400);
            if (createTeamRequest.Players.Any(player => string.IsNullOrWhiteSpace(player.Name) || player.Number == null))
                return BaseResponse<Team>.Fail($"Some Team Members have invalid Names or Numbers. Players: '{JsonConvert.SerializeObject(createTeamRequest.Players.FirstOrDefault(player => string.IsNullOrWhiteSpace(player.Name) || player.Number == null) ?? new Models.Dtos.BulkInsertPlayer())}'.", 400);

            TeamCommand newTeamCommand = new();
            Team newTeam = new();

            newTeamCommand.Name = createTeamRequest.TeamName;
            newTeamCommand.ManagerId = user.Id;
            newTeamCommand.SeasonId = createTeamRequest.SeasonId;

            try { newTeam = await _teamRepository.CreateTeam(newTeamCommand); }
            catch (Exception ex) { throw new Exception($"TeamService.CreateTeam Error: Failed to Create Team. Request: '{JsonConvert.SerializeObject(createTeamRequest)}'. ErrorMessage: '{ex.Message}'"); }

            try
            {
                createTeamRequest.Players.ForEach(player => { player.TeamId = newTeam.Id; player.SeasonId = createTeamRequest.SeasonId; });
                await _playerEfcRepository.BulkInsertPlayersForTeam(createTeamRequest.Players);
            }
            catch (Exception ex)
            {
                await _teamRepository.DeleteTeamById(newTeam.Id);
                return BaseResponse<Team>.Fail($"TeamService.CreateTeam Error: Failed to BulkInsert Players. Request: '{JsonConvert.SerializeObject(createTeamRequest)}'. ErrorMessage: '{ex.Message}'.", 500);
            }

            try { await _seasonPassRepository.UseSeasonPass(new() { Id = seasonPass.Id, ManagerId = user.Id, TeamId = newTeam.Id }); }
            catch (Exception ex) { _logger.LogError($"TeamService-CreateTeam-UseSeasonPass failed. SeasonPassId: '{seasonPass.Id}', ManagerId: '{user.Id}', TeamId: '{newTeam.Id}', ErrorMessage: '{ex.Message}'."); }

            return BaseResponse<Team>.Success(newTeam, 200);
        }

        public async Task<BaseResponse<IEnumerable<Team>>> GetTeamsFromTheSeason(int? seasonId)
        {
            seasonId ??= _seasonsSettings.Value.CurrentSeasonId ?? await _seasonRepository.GetLastSeasonId();

            var result = await _teamRepository.GetTeamsBySeasonId(seasonId ?? 0);
            return BaseResponse<IEnumerable<Team>>.Success(result, 200);
        }

        public async Task<SeasonTeamStandings> GetTeamStats(int? teamId)
        {
            return await _teamRepository.GetTeamStats(teamId ?? 0);
        }

        public async Task<CreateTeamRequestForm> GetCreateTeamRequest(int? seasonId, int? premadeTeamId)
        {
            if (seasonId == null || seasonId == 0)
                seasonId = _seasonsSettings.Value.CurrentSeasonId ?? await _seasonRepository.GetLastSeasonId();

            CreateTeamRequestForm newRequest = new();
            PremadeTeam premadeTeam = PreMadeTeams.Teams?.FirstOrDefault(team => team.Id == premadeTeamId);

            // TODO - How to fix this form...

            newRequest.TeamName = "";
            newRequest.UserUsername = "";
            newRequest.UserPassword = "";
            newRequest.SeasonId = seasonId;
            newRequest.SeasonPassPassword = "";

            if (premadeTeam != null)
            {
                newRequest.Player1Name = premadeTeam.Players[0].Name;
                newRequest.Player1Position = premadeTeam.Players[0].Position;
                newRequest.Player1Number = premadeTeam.Players[0].Number;

                newRequest.Player2Name = premadeTeam.Players[1].Name;
                newRequest.Player2Position = premadeTeam.Players[1].Position;
                newRequest.Player2Number = premadeTeam.Players[1].Number;

                newRequest.Player3Name = premadeTeam.Players[2].Name;
                newRequest.Player3Position = premadeTeam.Players[2].Position;
                newRequest.Player3Number = premadeTeam.Players[2].Number;

                newRequest.Player4Name = premadeTeam.Players[3].Name;
                newRequest.Player4Position = premadeTeam.Players[3].Position;
                newRequest.Player4Number = premadeTeam.Players[3].Number;

                newRequest.Player5Name = premadeTeam.Players[4].Name;
                newRequest.Player5Position = premadeTeam.Players[4].Position;
                newRequest.Player5Number = premadeTeam.Players[4].Number;

                newRequest.Player6Name = premadeTeam.Players[5].Name;
                newRequest.Player6Position = premadeTeam.Players[5].Position;
                newRequest.Player6Number = premadeTeam.Players[5].Number;

                newRequest.Player7Name = premadeTeam.Players[6].Name;
                newRequest.Player7Position = premadeTeam.Players[6].Position;
                newRequest.Player7Number = premadeTeam.Players[6].Number;

                newRequest.Player8Name = premadeTeam.Players[7].Name;
                newRequest.Player8Position = premadeTeam.Players[7].Position;
                newRequest.Player8Number = premadeTeam.Players[7].Number;

                newRequest.Player9Name = premadeTeam.Players[8].Name;
                newRequest.Player9Position = premadeTeam.Players[8].Position;
                newRequest.Player9Number = premadeTeam.Players[8].Number;

                newRequest.Player10Name = premadeTeam.Players[9].Name;
                newRequest.Player10Position = premadeTeam.Players[9].Position;
                newRequest.Player10Number = premadeTeam.Players[9].Number;

                newRequest.Player11Name = premadeTeam.Players[10].Name;
                newRequest.Player11Position = premadeTeam.Players[10].Position;
                newRequest.Player11Number = premadeTeam.Players[10].Number;
            }

            return newRequest;
        }

        public async Task<BaseResponse<Team>> CreateTeamWithForm(CreateTeamRequestForm createTeamRequestForm)
        {
            CreateTeamRequest createTeamRequest = new();
            createTeamRequest.TeamName = createTeamRequestForm.TeamName;
            createTeamRequest.Players = new();
            createTeamRequest.UserUsername = createTeamRequestForm.UserUsername;
            createTeamRequest.UserPassword = createTeamRequestForm.UserPassword;
            createTeamRequest.SeasonId = createTeamRequestForm.SeasonId;
            createTeamRequest.SeasonPassPassword = createTeamRequestForm.SeasonPassPassword;

            // TODO - NEED TO FIX THIS

            BulkInsertPlayer newPlayer1 = new();
            BulkInsertPlayer newPlayer2 = new();
            BulkInsertPlayer newPlayer3 = new();
            BulkInsertPlayer newPlayer4 = new();
            BulkInsertPlayer newPlayer5 = new();
            BulkInsertPlayer newPlayer6 = new();
            BulkInsertPlayer newPlayer7 = new();
            BulkInsertPlayer newPlayer8 = new();
            BulkInsertPlayer newPlayer9 = new();
            BulkInsertPlayer newPlayer10 = new();
            BulkInsertPlayer newPlayer11 = new();

            newPlayer1.Name = createTeamRequestForm.Player1Name;
            newPlayer1.Position = createTeamRequestForm.Player1Position;
            newPlayer1.Number = createTeamRequestForm.Player1Number;
            newPlayer1.TeamId = createTeamRequestForm.TeamId;
            newPlayer1.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer1);

            newPlayer2.Name = createTeamRequestForm.Player2Name;
            newPlayer2.Position = createTeamRequestForm.Player2Position;
            newPlayer2.Number = createTeamRequestForm.Player2Number;
            newPlayer2.TeamId = createTeamRequestForm.TeamId;
            newPlayer2.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer2);

            newPlayer3.Name = createTeamRequestForm.Player3Name;
            newPlayer3.Position = createTeamRequestForm.Player3Position;
            newPlayer3.Number = createTeamRequestForm.Player3Number;
            newPlayer3.TeamId = createTeamRequestForm.TeamId;
            newPlayer3.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer3);

            newPlayer4.Name = createTeamRequestForm.Player4Name;
            newPlayer4.Position = createTeamRequestForm.Player4Position;
            newPlayer4.Number = createTeamRequestForm.Player4Number;
            newPlayer4.TeamId = createTeamRequestForm.TeamId;
            newPlayer4.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer4);

            newPlayer5.Name = createTeamRequestForm.Player5Name;
            newPlayer5.Position = createTeamRequestForm.Player5Position;
            newPlayer5.Number = createTeamRequestForm.Player5Number;
            newPlayer5.TeamId = createTeamRequestForm.TeamId;
            newPlayer5.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer5);

            newPlayer6.Name = createTeamRequestForm.Player6Name;
            newPlayer6.Position = createTeamRequestForm.Player6Position;
            newPlayer6.Number = createTeamRequestForm.Player6Number;
            newPlayer6.TeamId = createTeamRequestForm.TeamId;
            newPlayer6.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer6);

            newPlayer7.Name = createTeamRequestForm.Player7Name;
            newPlayer7.Position = createTeamRequestForm.Player7Position;
            newPlayer7.Number = createTeamRequestForm.Player7Number;
            newPlayer7.TeamId = createTeamRequestForm.TeamId;
            newPlayer7.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer7);

            newPlayer8.Name = createTeamRequestForm.Player8Name;
            newPlayer8.Position = createTeamRequestForm.Player8Position;
            newPlayer8.Number = createTeamRequestForm.Player8Number;
            newPlayer8.TeamId = createTeamRequestForm.TeamId;
            newPlayer8.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer8);

            newPlayer9.Name = createTeamRequestForm.Player9Name;
            newPlayer9.Position = createTeamRequestForm.Player9Position;
            newPlayer9.Number = createTeamRequestForm.Player9Number;
            newPlayer9.TeamId = createTeamRequestForm.TeamId;
            newPlayer9.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer9);

            newPlayer10.Name = createTeamRequestForm.Player10Name;
            newPlayer10.Position = createTeamRequestForm.Player10Position;
            newPlayer10.Number = createTeamRequestForm.Player10Number;
            newPlayer10.TeamId = createTeamRequestForm.TeamId;
            newPlayer10.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer10);

            newPlayer11.Name = createTeamRequestForm.Player11Name;
            newPlayer11.Position = createTeamRequestForm.Player11Position;
            newPlayer11.Number = createTeamRequestForm.Player11Number;
            newPlayer11.TeamId = createTeamRequestForm.TeamId;
            newPlayer11.SeasonId = createTeamRequestForm.SeasonId;

            createTeamRequest.Players.Add(newPlayer11);

            return await CreateTeam(createTeamRequest);
        }
    }
}
