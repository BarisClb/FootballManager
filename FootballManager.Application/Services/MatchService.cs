using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Dtos;
using FootballManager.Application.Models.Responses;
using FootballManager.Application.Settings;
using FootballManager.Application.Settings.Chances;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FootballManager.Application.Services
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IGoalRepository _goalRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<MatchService> _logger;
        private readonly IOptions<SeasonsSettings> _seasonsSettings;

        public MatchService(IMatchRepository matchRepository, IGoalRepository goalRepository, IPlayerRepository playerRepository, IUserRepository userRepository, ILogger<MatchService> logger, IOptions<SeasonsSettings> seasonsSettings)
        {
            _matchRepository = matchRepository ?? throw new ArgumentNullException(nameof(matchRepository));
            _goalRepository = goalRepository ?? throw new ArgumentNullException(nameof(goalRepository));
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _seasonsSettings = seasonsSettings;
        }


        public async Task<Match> CreateMatch(MatchCommand createMatchCommand)
        {
            return await _matchRepository.CreateMatch(createMatchCommand);
        }

        public async Task<BaseResponse<Match>> SimulateMatch(int matchId)
        {
            // Get Match and Player info
            var match = await _matchRepository.GetMatchById(matchId);
            if (match == null) { return BaseResponse<Match>.Fail("Match does not exist.", 400); }

            var playerList = await _playerRepository.GetPlayersByMatchId(matchId);
            var homeTeam = playerList.Where(player => player.TeamId == match.HomeTeamId).ToList();
            var awayTeam = playerList.Where(player => player.TeamId == match.AwayTeamId).ToList();

            if (playerList == null || !playerList.Any()) { return BaseResponse<Match>.Fail($"No players Registered for The Match: '{JsonConvert.SerializeObject(match)}'", 400); }
            if (homeTeam == null || !homeTeam.Any()) { return BaseResponse<Match>.Fail($"No players Registered for the Home team. The Match: '{JsonConvert.SerializeObject(match)}'", 400); }
            if (awayTeam == null || !awayTeam.Any()) { return BaseResponse<Match>.Fail($"No players Registered for the Away team. The Match: '{JsonConvert.SerializeObject(match)}'", 400); }

            // Generate Goal Chances

            List<int> goalChancePool = await generateTeamGoalChances();
            List<int> scoreChancePool = await generatePlayerScoreChances();
            List<int> assistChancePool = await generatePlayerAssistChances();

            // Generate Goal Numbers

            Random _random = new();

            int homeTeamFullTimeGoals = goalChancePool[_random.Next(0, goalChancePool.Count)];
            int? homeTeamOvertimeGoals = null;
            int? homeTeamPenaltyGoals = null;
            // Home Team Advantage - Add 1 more Goal to Scored
            if (match.MatchType == Domain.Enums.MatchType.League)
                if (_random.Next(1, 101) <= (_seasonsSettings.Value.HomeTeamExtraGoalChance ?? 20))
                    homeTeamFullTimeGoals++;

            int awayTeamFullTimeGoals = goalChancePool[_random.Next(0, goalChancePool.Count)];
            int? awayTeamOvertimeGoals = null;
            int? awayTeamPenaltyGoals = null;


            // Register Goals

            if (homeTeamFullTimeGoals > 0)
                await registerGoals(matchId, homeTeam, match.HomeTeamId, match.AwayTeamId, homeTeamFullTimeGoals, scoreChancePool, assistChancePool);
            if (awayTeamFullTimeGoals > 0)
                await registerGoals(matchId, awayTeam, match.AwayTeamId, match.HomeTeamId, awayTeamFullTimeGoals, scoreChancePool, assistChancePool);

            // GoldenGoal for Finals and Tournaments
            if (homeTeamFullTimeGoals == awayTeamFullTimeGoals && (match.MatchType == Domain.Enums.MatchType.GoldenGoal || match.MatchType == Domain.Enums.MatchType.Tournament))
            {
                var scoredSide = _random.Next(1, 3);
                if (scoredSide == 1)
                {
                    homeTeamOvertimeGoals = 1;
                    await registerTieBreakerGoal(matchId, homeTeam, match.HomeTeamId, match.AwayTeamId, goalChancePool, assistChancePool);
                }
                else
                {
                    awayTeamOvertimeGoals = 1;
                    await registerTieBreakerGoal(matchId, awayTeam, match.AwayTeamId, match.HomeTeamId, goalChancePool, assistChancePool);
                }

            }

            // Conclude Match
            MatchCommand matchResult = new() { Id = matchId };
            int homeTeamFinalScore = homeTeamFullTimeGoals + (homeTeamOvertimeGoals ?? 0);
            int awayTeamFinalScore = awayTeamFullTimeGoals + (awayTeamOvertimeGoals ?? 0);
            MatchResultType? winner = null;

            // TODO - Penalty Shootout / Penalties

            if (homeTeamPenaltyGoals != awayTeamPenaltyGoals)
                winner = homeTeamPenaltyGoals > awayTeamPenaltyGoals ? MatchResultType.HomeTeamWin : MatchResultType.HomeTeamWin;
            else
                winner = homeTeamFinalScore > awayTeamFinalScore ? MatchResultType.HomeTeamWin : (awayTeamFinalScore > homeTeamFinalScore) ? MatchResultType.AwayTeamWin : MatchResultType.Tie;

            matchResult.HomeTeamFullTimeScore = homeTeamFullTimeGoals;
            matchResult.AwayTeamFullTimeScore = awayTeamFullTimeGoals;
            matchResult.HomeTeamOvertimeScore = homeTeamOvertimeGoals;
            matchResult.AwayTeamOvertimeScore = awayTeamOvertimeGoals;
            matchResult.HomeTeamPenaltyScore = homeTeamPenaltyGoals;
            matchResult.AwayTeamPenaltyScore = awayTeamPenaltyGoals;
            matchResult.Winner = winner;

            Match result = null;
            try { result = await _matchRepository.ConcludeMatch(matchResult); }
            catch (Exception ex)
            {
                await _goalRepository.DeleteGoalsByMatchId(matchId);
                _logger.LogError($"MatchService-SimulateMatch-ConcludeMatch. MatchId: {matchId} MatchResult: '{JsonConvert.SerializeObject(matchResult)}', ErrorMessage: '{ex.Message}'.");
                return BaseResponse<Match>.Fail($"Failed to Conclude Match. The Match: '{JsonConvert.SerializeObject(match)}'", 400);
            }


            // Send Email to Managers
            var managers = await _userRepository.GetManagersByMatchId(matchId);
            foreach (var manager in managers)
            {
                if (!string.IsNullOrEmpty(manager.Email))
                {
                    string matchResultEmail = $"{homeTeam.FirstOrDefault()?.TeamName ?? ""}  {homeTeamFinalScore} - {awayTeamFinalScore}  {awayTeam.FirstOrDefault()?.TeamName ?? ""}";
                    BackgroundJob.Enqueue<IEmailService>(service => service.SendMatchResult(matchResultEmail, result.DatePlayed ?? DateTime.UtcNow, manager.Email));
                }
            }

            BackgroundJob.Enqueue<IJobService>(service => service.ConcludeSeasons());

            return BaseResponse<Match>.Success(result, 200);
        }


        private async Task registerGoals(int matchId, List<PlayerListOfTheMatch> scorerTeam, int scorerTeamId, int concededTeamId, int goalCount, List<int> scoreChancePool, List<int> assistChancePool)
        {
            //var matchId = scorerTeam.FirstOrDefault(player => true).MatchId; // What if I wanted to remove scorerTeam Dto's MatchId field? This Creates a Dependency
            //var scorerTeamId = scorerTeam.FirstOrDefault(player => true).TeamId; // What if I wanted to remove scorerTeam Dto's TeamId field? This Creates a Dependency
            Random _random = new();

            #region SortScoreTimes
            List<int> goalTimes = new();
            for (var x = 0; x < goalCount; x++)
                goalTimes.Add(_random.Next(1, 91));
            goalTimes.Sort((a, b) => a - b);
            #endregion

            for (var i = 0; i < goalCount; i++)
            {
                GoalCommand newGoal = new();

                #region Scorer and Assist
                int scoreByPosition = scoreChancePool[_random.Next(0, scoreChancePool.Count)];
                int? assistByPosition = null;

                // Should All Goals have Assists?
                if (_seasonsSettings.Value.AllowNoAssistGoals == true)
                {
                    assistByPosition = assistChancePool[_random.Next(0, assistChancePool.Count)];
                    if (scoreByPosition == assistByPosition)
                        assistByPosition = null;
                }
                else
                {
                    var assistPoolWithoutScorer = assistChancePool.Where(position => position != scoreByPosition).ToList();
                    assistByPosition = assistPoolWithoutScorer[_random.Next(0, assistPoolWithoutScorer.Count)];
                }
                #endregion

                newGoal.MinuteScored = goalTimes[i];
                newGoal.MatchId = matchId;
                newGoal.TeamScoredId = scorerTeamId;
                newGoal.TeamConcededId = concededTeamId;
                newGoal.ScoredById = scorerTeam.FirstOrDefault(player => player.PlayerPosition == scoreByPosition)?.PlayerId;
                newGoal.AssistedById = assistByPosition != null ? scorerTeam.FirstOrDefault(player => player.PlayerPosition == assistByPosition)?.PlayerId : null;

                await _goalRepository.CreateGoal(newGoal);
            }
        }

        private async Task registerTieBreakerGoal(int matchId, List<PlayerListOfTheMatch> scorerTeam, int scorerTeamId, int concededTeamId, List<int> scoreChancePool, List<int> assistChancePool)
        {
            GoalCommand newGoal = new();
            Random _random = new();

            #region Scorer and Assist
            int scoreByPosition = scoreChancePool[_random.Next(0, scoreChancePool.Count)];
            int? assistByPosition = null;

            // Should All Goals have Assists?
            if (_seasonsSettings.Value.AllowNoAssistGoals ?? true)
            {
                assistByPosition = scoreChancePool[_random.Next(0, assistChancePool.Count)];
                if (scoreByPosition == assistByPosition)
                    assistByPosition = null;
            }
            else
            {
                var assistPoolWithoutScorer = assistChancePool.Where(position => position != scoreByPosition).ToList();
                assistByPosition = assistPoolWithoutScorer[_random.Next(0, assistPoolWithoutScorer.Count)];
            }
            #endregion

            newGoal.MinuteScored = _random.Next(91, 121);
            newGoal.MatchId = matchId;
            newGoal.TeamScoredId = scorerTeamId;
            newGoal.TeamConcededId = concededTeamId;
            newGoal.ScoredById = scorerTeam.FirstOrDefault(player => player.PlayerPosition == scoreByPosition)?.PlayerId;
            newGoal.ScoredById = assistByPosition != null ? scorerTeam.FirstOrDefault(player => player.PlayerPosition == assistByPosition)?.PlayerId : null;

            await _goalRepository.CreateGoal(newGoal);
        }


        private async Task<List<int>> generateTeamGoalChances()
        {
            List<int> goalChancePool = new();
            foreach (var goalChances in TeamChancesSettings.GoalChances)
            {
                int added = 0;
                while (goalChances.chanceToScore > added)
                {
                    goalChancePool.Add(goalChances.numberOfGoals);
                    added++;
                }
            }
            return goalChancePool;
        }

        private async Task<List<int>> generatePlayerScoreChances()
        {
            List<int> scoreChancePool = new();
            foreach (var scoreChances in PlayerChancesSettings.ChancesToScore)
            {
                int added = 0;
                while (scoreChances.chanceToScore > added)
                {
                    scoreChancePool.Add(scoreChances.playerPosition);
                    added++;
                }
            }
            return scoreChancePool;
        }

        private async Task<List<int>> generatePlayerAssistChances()
        {
            List<int> assistChancePool = new();
            foreach (var assistChances in PlayerChancesSettings.ChancesToAssist)
            {
                int added = 0;
                while (assistChances.chanceToAssist > added)
                {
                    assistChancePool.Add(assistChances.playerPosition);
                    added++;
                }
            }
            return assistChancePool;
        }
    }
}
