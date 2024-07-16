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
using FootballManager.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace FootballManager.Application.Services
{
    public class SeasonService : ISeasonService
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly IMatchEfcRepository _matchEfcRepository;
        private readonly IOptions<SeasonsSettings> _seasonsSettings;
        private readonly IOptions<ProjectSettings> _projectSettings;
        private readonly ILogger<SeasonService> _logger;

        public SeasonService(ISeasonRepository seasonRepository, ITeamRepository teamRepository, IMatchRepository matchRepository, IMatchEfcRepository matchEfcRepository, IOptions<SeasonsSettings> seasonsSettings, IOptions<ProjectSettings> projectSettings, ILogger<SeasonService> logger)
        {
            _seasonRepository = seasonRepository ?? throw new ArgumentNullException(nameof(seasonRepository));
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _matchRepository = matchRepository ?? throw new ArgumentNullException(nameof(matchRepository));
            _matchEfcRepository = matchEfcRepository ?? throw new ArgumentNullException(nameof(matchEfcRepository));
            _seasonsSettings = seasonsSettings ?? throw new ArgumentNullException(nameof(seasonsSettings));
            _projectSettings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<Season> CreateSeason(CreateSeasonRequest createSeasonRequest)
        {
            if (createSeasonRequest.Name.EndsWith("(WL)") || createSeasonRequest.Name.EndsWith("(WT)"))
                throw new Exception("SeasonName can't end with (WL) or (Wt)");
            return await _seasonRepository.CreateSeason(createSeasonRequest);
        }

        public async Task<BaseResponse<bool>> ScheduleSeason(ScheduleSeasonRequest scheduleSeasonRequest)
        {
            if (scheduleSeasonRequest.SeasonType == SeasonType.League)
                return await ScheduleLeagueSeason(scheduleSeasonRequest);
            else if (scheduleSeasonRequest.SeasonType == SeasonType.Tournament)
                return await ScheduleTournamentSeason(scheduleSeasonRequest);
            return BaseResponse<bool>.Fail($"Please provide a valid SeasonType. SeasonType: {scheduleSeasonRequest.SeasonType}", 400);
        }

        public async Task<BaseResponse<bool>> ScheduleLeagueSeason(ScheduleSeasonRequest scheduleSeasonRequest)
        {
            scheduleSeasonRequest.SeasonId ??= _seasonsSettings.Value.CurrentSeasonId ?? await _seasonRepository.GetLastSeasonId();
            scheduleSeasonRequest.SeasonType ??= SeasonType.League;

            var season = await _seasonRepository.GetSeasonById(scheduleSeasonRequest.SeasonId ?? 0);
            if (season == null)
                return BaseResponse<bool>.Fail($"Failed to find Season. SeasonId: {scheduleSeasonRequest.SeasonId}.", 400);

            var matches = await _matchRepository.GetMatchesBySeasonId(scheduleSeasonRequest.SeasonId ?? season.Id);
            if (matches != null && matches.Any())
                return BaseResponse<bool>.Fail($"There are already Matches for this Season. SeasonId: {scheduleSeasonRequest.SeasonId}.", 400);

            var teams = (await _teamRepository.GetTeamsBySeasonId(scheduleSeasonRequest.SeasonId ?? 0))?.ToList();
            if (teams == null || !teams.Any())
                return BaseResponse<bool>.Fail($"Failed to find any teams for Season. SeasonId: {scheduleSeasonRequest.SeasonId}.", 400);

            if (teams.Count < 2)
                return BaseResponse<bool>.Fail($"Can't start a Season with less than 2 teams: {JsonConvert.SerializeObject(teams)}.", 400);

            // Validate Match Times

            scheduleSeasonRequest.MatchIntervalMinutes ??= _seasonsSettings.Value.DefaultMatchIntervalMinutes ?? 0;

            if (scheduleSeasonRequest.DailyMatchHours == null || scheduleSeasonRequest.DailyMatchHours.Count == 0)
                scheduleSeasonRequest.DailyMatchHours = _seasonsSettings.Value.DefaultMatchTimes;

            // HH:MM:SS Regex (Seconds are optional) => ([0-1]?[0-9]|[2][0-3]):([0-5][0-9])(:[0-5][0-9])?$
            // HH:MM Regex => ([0-1]?[0-9]|[2][0-3]):([0-5][0-9])$
            Regex hourRegex = new(@"^([0-1]?[0-9]|[2][0-3]):([0-5][0-9])$");
            if (!scheduleSeasonRequest.DailyMatchHours.All(time => hourRegex.Match(time).Success))
                return BaseResponse<bool>.Fail($"Match times need to be in following format: HH:MM", 400);

            // Adjust Match Times to TimeDifference
            if (_projectSettings.Value.TimezoneDifference != null && _projectSettings.Value.TimezoneDifference != 0)
            {
                for (var a = 0; a < scheduleSeasonRequest.DailyMatchHours.Count; a++)
                {
                    var hour = Int32.Parse(scheduleSeasonRequest.DailyMatchHours[a].Split(':')[0]);
                    var minute = Int32.Parse(scheduleSeasonRequest.DailyMatchHours[a].Split(':')[1]);
                    hour -= _projectSettings.Value.TimezoneDifference ?? 0;

                    if (hour < 0)
                        hour += 24;
                    else if (hour > 24)
                        hour -= 24;

                    scheduleSeasonRequest.DailyMatchHours[a] = $"{hour:00}:{minute:00}";
                }
            }

            scheduleSeasonRequest.DailyMatchHours = scheduleSeasonRequest.DailyMatchHours.Order().ToList();

            if (scheduleSeasonRequest.DailyMatchHours.Count > 1)
            {
                // Check if Match Times Collides with each other
                for (var a = 1; a < scheduleSeasonRequest.DailyMatchHours.Count; a++)
                {
                    var firstTimeHour = Int32.Parse(scheduleSeasonRequest.DailyMatchHours[a - 1].Split(':')[0]);
                    var firstTimeMinute = Int32.Parse(scheduleSeasonRequest.DailyMatchHours[a - 1].Split(':')[1]);

                    var secondTimeHour = Int32.Parse(scheduleSeasonRequest.DailyMatchHours[a].Split(':')[0]);
                    var secondTimeMinute = Int32.Parse(scheduleSeasonRequest.DailyMatchHours[a].Split(':')[1]);

                    var firstTime = (firstTimeHour * 60) + firstTimeMinute;
                    var secondTime = (secondTimeHour * 60) + secondTimeMinute;

                    var difference = secondTime - firstTime;
                    var matchesPerRound = teams.Count / 2;

                    if ((matchesPerRound * scheduleSeasonRequest.MatchIntervalMinutes) > difference)
                        return BaseResponse<bool>.Fail($"Match time collision. Either decrease the MatchIntervalMinutes: '{scheduleSeasonRequest.MatchIntervalMinutes}' or increase time between match hours: {scheduleSeasonRequest.DailyMatchHours[a - 1]} - {scheduleSeasonRequest.DailyMatchHours[a]}", 400);
                }
            }

            // Generate Matches By Season Half

            List<int> teamIdList = new();
            teams.ForEach(team => teamIdList.Add(team.Id));
            Random random = new();
            teamIdList = teamIdList.OrderBy(a => random.Next()).ToList();

            List<(int team1, int team2)> firstHalfMatches = new();
            List<(int team1, int team2)> secondHalfMatches = new();

            List<MatchWithDate> firstHalfMatchesByRound = new();
            List<MatchWithDate> secondHalfMatchesByRound = new();

            // After iterating the first number(x) and pass to the next(y), the last instance the(y) was used was as the second int inside the tuple(with the number just behind it), so we want to always start iterating the new number as the first side of the tuple and then continue to switch it as we iterate            
            for (var x = 0; x < teamIdList.Count; x++)
            {
                var count = 0;
                for (var y = x + 1; y < teamIdList.Count; y++)
                {
                    if (count % 2 == 0)
                    {
                        firstHalfMatches.Add((teamIdList[x], teamIdList[y]));
                        secondHalfMatches.Add((teamIdList[y], teamIdList[x]));
                    }
                    else
                    {
                        secondHalfMatches.Add((teamIdList[x], teamIdList[y]));
                        firstHalfMatches.Add((teamIdList[y], teamIdList[x]));
                    }
                    count++;
                }
            }

            // Generate Matches By Season Rounds

            // TODO - THIS NEEDS FIXING - Note: 'With current logic, Each round a single team is left out and we generate matches between the other teams. BUT, the code right now ignores the matchup. For Example: In a 5 team League: Team A doesn't play first round but we are not taking into account if the matches should be B-C and D-E or B-D and C-E or B-E and C-D, Which sometimes causes missing matches from last rounds. Until I fix it, I will implement a 'Reroll' system.'
            int halfSeasonWeekCount = teams.Count % 2 == 0 ? (teams.Count - 1) : teams.Count;
            bool reroll1 = true;
            bool reroll2 = true;

            var i = 1;
            while (reroll1)
            {
                i = 1;
                List<(int team1, int team2)> firstHalfMatchesTemp = new();
                firstHalfMatches = firstHalfMatches.OrderBy(a => random.Next()).ToList();
                firstHalfMatches.ForEach(item => firstHalfMatchesTemp.Add(item));
                while (i <= halfSeasonWeekCount) // First Half
                {
                    List<int> playedThisRound = new();
                    // If there are odd number of teams, we need to skip one team every round. I will use round number == index number(of teamList) to filter a single team
                    if (teamIdList.Count % 2 == 1)
                        playedThisRound.Add(teamIdList[i - 1]);

                    for (var y = 0; y < teamIdList.Count; y++)
                    {
                        var id = teamIdList[y];
                        if (playedThisRound.Contains(id))
                            continue;

                        var match = firstHalfMatchesTemp.FirstOrDefault(match => (match.team1 == id && !playedThisRound.Contains(match.team2)) || (match.team2 == id && !playedThisRound.Contains(match.team1)));
                        if (match.team1 == 0)
                            continue;

                        firstHalfMatchesByRound.Add(new MatchWithDate() { Round = i, Date = null, Team1 = match.team1, Team2 = match.team2 });
                        //firstHalfMatches.Remove(match);
                        firstHalfMatchesTemp.Remove(match);
                        playedThisRound.Add(match.team1);
                        playedThisRound.Add(match.team2);
                    }
                    i++;
                }
                if (firstHalfMatchesByRound.Count == firstHalfMatches.Count)
                    reroll1 = false;
                else
                    firstHalfMatchesByRound = new();
            }

            teamIdList = teamIdList.OrderBy(a => random.Next()).ToList();
            while (reroll2)
            {
                i = halfSeasonWeekCount + 1;
                List<(int team1, int team2)> secondHalfMatchesTemp = new();
                secondHalfMatches = secondHalfMatches.OrderBy(a => random.Next()).ToList();
                secondHalfMatches.ForEach(item => secondHalfMatchesTemp.Add(item));
                while (i <= halfSeasonWeekCount * 2) // Second Half
                {
                    List<int> playedThisRound = new();
                    // If there are odd number of teams, we need to skip one team every round. I will use round number == index number(of teamList) to filter a single team
                    if (teamIdList.Count % 2 == 1)
                        playedThisRound.Add(teamIdList[i - 1 - teamIdList.Count]);

                    for (var y = 0; y < teamIdList.Count; y++)
                    {
                        var id = teamIdList[y];
                        if (playedThisRound.Contains(id))
                            continue;

                        var match = secondHalfMatchesTemp.FirstOrDefault(match => (match.team1 == id && !playedThisRound.Contains(match.team2)) || (match.team2 == id && !playedThisRound.Contains(match.team1)));
                        if (match.team2 == 0)
                            continue;

                        secondHalfMatchesByRound.Add(new MatchWithDate() { Round = i, Date = null, Team1 = match.team1, Team2 = match.team2 });
                        //secondHalfMatches.Remove(match);
                        secondHalfMatchesTemp.Remove(match);
                        playedThisRound.Add(match.team1);
                        playedThisRound.Add(match.team2);
                    }
                    i++;
                }
                if (secondHalfMatchesByRound.Count == secondHalfMatches.Count)
                    reroll2 = false;
                else
                    secondHalfMatchesByRound = new();
            }

            // Generate Match Dates

            DateTime matchDate = scheduleSeasonRequest.FirstMatchAfter == null ? DateTime.UtcNow : ((DateTime)scheduleSeasonRequest.FirstMatchAfter?.AddHours((_projectSettings.Value.TimezoneDifference ?? 0) * -1));
            var newMatchDate = matchDate;

            while (firstHalfMatchesByRound.Any(match => match.Date == null)) // First Half
            {
                foreach (var time in scheduleSeasonRequest.DailyMatchHours)
                {
                    var hour = Int32.Parse(time.Split(':')[0]);
                    var minute = Int32.Parse(time.Split(':')[1]);
                    newMatchDate = new DateTime(matchDate.Year, matchDate.Month, matchDate.Day, hour, minute, 0);

                    if (newMatchDate > matchDate)
                    {
                        var matchRound = firstHalfMatchesByRound.FirstOrDefault(match => match.Date == null)?.Round;
                        if (matchRound != null)
                        {
                            foreach (var match in firstHalfMatchesByRound.Where(match => match.Date == null && match.Round == matchRound))
                            {
                                match.Date = newMatchDate;
                                newMatchDate = newMatchDate.AddMinutes(scheduleSeasonRequest.MatchIntervalMinutes ?? 0);
                            }
                        }
                        else
                        {
                            newMatchDate = newMatchDate.AddDays(-1); // AddDays: This neutralizes the bellow code before exiting this rotation. AddMinutes: If the last 'time' in foreach is not the last member, It will not be used in the next rotation (second half)
                            break;
                        }
                    }
                }
                newMatchDate = newMatchDate.AddMinutes(-1).AddDays(1); // Neutralizes this
                matchDate = new DateTime(newMatchDate.Year, newMatchDate.Month, newMatchDate.Day, 0, 0, 0);
            }

            // Date of the Last Match + MinimumIntervalHour => 

            matchDate = firstHalfMatchesByRound.Last().Date?.AddHours(scheduleSeasonRequest.SeasonHalvesMinimumIntervalHours ?? _seasonsSettings.Value.LeagueHalvesMinumumIntervalHours ?? 0) ?? newMatchDate.AddHours(scheduleSeasonRequest.SeasonHalvesMinimumIntervalHours ?? _seasonsSettings.Value.LeagueHalvesMinumumIntervalHours ?? 0);

            while (secondHalfMatchesByRound.Any(match => match.Date == null)) // Second Half
            {
                foreach (var time in scheduleSeasonRequest.DailyMatchHours)
                {
                    var hour = Int32.Parse(time.Split(':')[0]);
                    var minute = Int32.Parse(time.Split(':')[1]);
                    newMatchDate = new DateTime(matchDate.Year, matchDate.Month, matchDate.Day, hour, minute, 0);

                    if (newMatchDate > matchDate)
                    {
                        var matchRound = secondHalfMatchesByRound.FirstOrDefault(match => match.Date == null)?.Round;
                        if (matchRound != null)
                        {
                            foreach (var match in secondHalfMatchesByRound.Where(match => match.Date == null && match.Round == matchRound))
                            {
                                match.Date = newMatchDate;
                                newMatchDate = newMatchDate.AddMinutes(scheduleSeasonRequest.MatchIntervalMinutes ?? 0);
                            }
                        }
                        else
                            break;
                    }

                }
                newMatchDate = newMatchDate.AddMinutes(-1).AddDays(1);
                matchDate = new DateTime(newMatchDate.Year, newMatchDate.Month, newMatchDate.Day, 0, 0, 0);
            }

            // Create Matches

            List<BulkInsertMatch> allMatches = new();
            List<MatchWithDate> allMatchesByRound = new();
            allMatchesByRound.AddRange(firstHalfMatchesByRound);
            allMatchesByRound.AddRange(secondHalfMatchesByRound);

            for (var b = 0; b < allMatchesByRound.Count; b++)
            {
                BulkInsertMatch newMatch = new();
                newMatch.MatchType = scheduleSeasonRequest.SeasonType switch
                {
                    SeasonType.League => Domain.Enums.MatchType.League,
                    SeasonType.Tournament => Domain.Enums.MatchType.Tournament,
                    _ => throw new Exception("Invalid Season Type")
                };
                newMatch.MatchSeasonPeriodType = allMatchesByRound[b].Round <= (allMatchesByRound.GroupBy(match => match.Round).Count() / 2) ? MatchSeasonPeriodType.FirstHalfOfLeague : MatchSeasonPeriodType.SecondHalfOfLeague;
                newMatch.SeasonRound = allMatchesByRound[b].Round;
                newMatch.DatePlanned = allMatchesByRound[b].Date;
                newMatch.SeasonId = season.Id;
                newMatch.HomeTeamManagerId = teams.FirstOrDefault(team => team.Id == allMatchesByRound[b].Team1)?.ManagerId;
                newMatch.HomeTeamId = allMatchesByRound[b].Team1;
                newMatch.AwayTeamManagerId = teams.FirstOrDefault(team => team.Id == allMatchesByRound[b].Team2)?.ManagerId;
                newMatch.AwayTeamId = allMatchesByRound[b].Team2;

                allMatches.Add(newMatch);
            }

            var createMatchesResult = await _matchEfcRepository.BulkInsertMatchesForSeason(allMatches);

            if (!createMatchesResult)
                return BaseResponse<bool>.Fail($"Failed to create Matches for Season: {scheduleSeasonRequest.SeasonId}. Team List: {JsonConvert.SerializeObject(allMatches)}", 400);

            try { await _seasonRepository.CloseSeasonRegistration(scheduleSeasonRequest.SeasonId ?? 0, (int?)scheduleSeasonRequest.SeasonType.Value ?? 0); }
            catch (Exception ex)
            {
                _logger.LogError($"SeasonService-ScheduleLeagueSeason-CloseSeasonRegistration failed. SeasonId: '{scheduleSeasonRequest.SeasonId}', SeasonType: '{scheduleSeasonRequest.SeasonType.Value}', ErrorMessage: '{ex.Message}'.");
                await _matchRepository.DeleteMatchesForSeason(season.Id);
                return BaseResponse<bool>.Fail($"Failed to CloseSeasonRegistration for Season: {scheduleSeasonRequest.SeasonId}. Team List: {JsonConvert.SerializeObject(allMatches)}", 500);
            }

            return BaseResponse<bool>.Success(true, 200);
        }

        public async Task<bool> CancelSeasonSchedule(int seasonId)
        {
            return await _seasonRepository.CancelSeasonSchedule(seasonId);
        }

        public async Task<List<SeasonTeamStandings>> GetSeasonTeamStandings(int? seasonId)
        {
            if (seasonId == null || seasonId == 0)
                seasonId = _seasonsSettings.Value.CurrentSeasonId ?? await _seasonRepository.GetLastSeasonId();
            if (seasonId == null || seasonId == 0)
                return new();

            var teamStandings = await _seasonRepository.GetSeasonTeamStandings(seasonId ?? 0);
            return teamStandings.OrderBy(team => team.SeasonPlacement ?? 0).ThenByDescending(team => team.PointTotal).ThenByDescending(team => team.GoalAvarageTotal).ThenByDescending(team => team.ScoredTotal).ToList();
        }

        public async Task<List<SeasonPlayerStandings>> GetSeasonPlayerStandings(int? seasonId)
        {
            if (seasonId == null || seasonId == 0)
                seasonId = _seasonsSettings.Value.CurrentSeasonId ?? await _seasonRepository.GetLastSeasonId();
            if (seasonId == null || seasonId == 0)
                return new();

            var playerStandings = await _seasonRepository.GetSeasonPlayerStandings(seasonId ?? 0);
            return playerStandings.OrderByDescending(player => player.GoalTotal).ThenByDescending(player => player.GoalsScored).ThenByDescending(player => player.GoalsAssisted).ToList();
        }

        public async Task<List<SeasonMatchFixtureGroup>> GetSeasonMatchFixture(int? seasonId)
        {
            if (seasonId == null || seasonId == 0)
                seasonId = _seasonsSettings.Value.CurrentSeasonId ?? await _seasonRepository.GetLastSeasonId();
            if (seasonId == null || seasonId == 0)
                return new();

            var matchFixture = (await _seasonRepository.GetSeasonMatchFixture(seasonId ?? 0)).ToList();
            // Timezone Adjustments
            matchFixture.ForEach(goal => { goal.MatchDatePlanned = goal.MatchDatePlanned.AddHours(_projectSettings.Value.TimezoneDifference ?? 0); goal.MatchDatePlayed = goal.MatchDatePlayed?.AddHours(_projectSettings.Value.TimezoneDifference ?? 0); });

            // Custom Grouping
            List<SeasonMatchFixtureGroup> seasonFixture = new();

            var rounds = matchFixture.GroupBy(goal => goal.SeasonRound).OrderBy(group => group.Key);
            foreach (var round in rounds)
            {
                SeasonMatchFixtureGroup newRound = new();
                newRound.SeasonName = matchFixture.FirstOrDefault()?.SeasonName;
                newRound.SeasonRound = round.Key;
                newRound.Matches = new();

                var matchesByRound = round.GroupBy(match => match.MatchId).OrderBy(match =>
                {
                    return match.FirstOrDefault()?.MatchDatePlayed != null
                    ? match.FirstOrDefault()?.MatchDatePlayed
                    : match.FirstOrDefault()?.MatchDatePlanned;
                });

                foreach (var match in matchesByRound)
                {
                    SeasonMatchFixtureGroupMatch newMatch = new();

                    var goalsByMatch = matchFixture.Where(goal => goal.MatchId == match.Key).OrderBy(goal => goal.GoalMinuteScored);
                    var matchToAdd = matchFixture.FirstOrDefault(goal => goal.MatchId == match.Key);

                    newMatch.MatchId = matchToAdd.MatchId;
                    newMatch.MatchType = matchToAdd.MatchType;
                    newMatch.MatchSeasonPeriodType = matchToAdd.MatchSeasonPeriodType;
                    newMatch.SeasonRound = matchToAdd.SeasonRound;
                    newMatch.MatchDatePlanned = matchToAdd.MatchDatePlanned;
                    newMatch.MatchDatePlayed = matchToAdd.MatchDatePlayed;
                    newMatch.MatchWinner = matchToAdd.MatchWinner;
                    newMatch.HomeTeamId = matchToAdd.HomeTeamId;
                    newMatch.HomeTeamName = matchToAdd.HomeTeamName;
                    newMatch.HomeTeamFullTimeScore = matchToAdd.HomeTeamFullTimeScore;
                    newMatch.HomeTeamOvertimeScore = matchToAdd.HomeTeamOvertimeScore;
                    newMatch.HomeTeamPenaltyScore = matchToAdd.HomeTeamPenaltyScore;
                    newMatch.HomeTeamFinalScore = matchToAdd.HomeTeamFinalScore;
                    newMatch.AwayTeamId = matchToAdd.AwayTeamId;
                    newMatch.AwayTeamName = matchToAdd.AwayTeamName;
                    newMatch.AwayTeamFullTimeScore = matchToAdd.AwayTeamFullTimeScore;
                    newMatch.AwayTeamOvertimeScore = matchToAdd.AwayTeamOvertimeScore;
                    newMatch.AwayTeamPenaltyScore = matchToAdd.AwayTeamPenaltyScore;
                    newMatch.AwayTeamFinalScore = matchToAdd.AwayTeamFinalScore;
                    newMatch.HomeTeamManagerId = matchToAdd.HomeTeamManagerId;
                    newMatch.HomeTeamManagerName = matchToAdd.HomeTeamManagerName;
                    newMatch.AwayTeamManagerId = matchToAdd.AwayTeamManagerId;
                    newMatch.AwayTeamManagerName = matchToAdd.AwayTeamManagerName;
                    newMatch.Goals = new();

                    foreach (var goal in goalsByMatch)
                    {
                        SeasonMatchFixtureGroupMatchGoal newGoal = new();

                        newGoal.GoalMinuteScored = goal.GoalMinuteScored;
                        newGoal.MatchSideScored = goal.MatchSideScored;
                        newGoal.GoalScoredById = goal.GoalScoredById;
                        newGoal.GoalScoredByName = goal.GoalScoredByName;
                        newGoal.GoalScoredByNumber = goal.GoalScoredByNumber;
                        newGoal.GoalAssistById = goal.GoalAssistById;
                        newGoal.GoalAssistByName = goal.GoalAssistByName;
                        newGoal.GoalAssistByNumber = goal.GoalAssistByNumber;

                        newMatch.Goals.Add(newGoal);
                    }

                    newRound.Matches.Add(newMatch);
                }

                seasonFixture.Add(newRound);
            }

            return seasonFixture;
        }

        public async Task<BaseResponse<string>> ConcludeLeague(int seasonId)
        {
            var season = await _seasonRepository.GetSeasonById(seasonId);
            if (season == null)
                return BaseResponse<string>.Fail($"Season not found. SeasonId: {seasonId}", 404);

            var teamStandings = (await _seasonRepository.GetSeasonTeamStandings(seasonId)).ToList();
            int halfSeasonWeekCount = teamStandings.Count % 2 == 0 ? (teamStandings.Count - 1) : teamStandings.Count;

            if (teamStandings == null)
                return BaseResponse<string>.Fail($"Failed to retrieve SeasonTeamStandings. SeasonId: {seasonId}", 500);

            if (teamStandings.Any(team => team.MatchesPlayedTotal < ((teamStandings.Count - 1) * 2)))
                return BaseResponse<string>.Fail($"League hasn't finished yet. SeasonId: {seasonId}", 400);

            // TODO - Scored Total is not relevant to Team Standings. As I didn't implemented a way to resolve 3-way Ties and ties below the first 2 places, I added this to make these situtations less unfair.
            teamStandings = teamStandings.OrderByDescending(team => team.PointTotal).ThenByDescending(team => team.GoalAvarageTotal).ThenByDescending(team => team.ScoredTotal).ToList();

            List<BulkUpdateSeasonPlacement> placementList = new();

            for (var i = 0; i < teamStandings.Count; i++)
            {
                BulkUpdateSeasonPlacement teamPlacement = new();
                var team = teamStandings[i];

                teamPlacement.Id = team.TeamId;
                teamPlacement.SeasonPlacement = i + 1;

                placementList.Add(teamPlacement);
            }

            // TieBreaker
            var firstPlaceTeam = teamStandings[0];
            var secondPlaceTeam = teamStandings[1];

            var tiebreakerMatch = await _matchRepository.GetSeasonTieBreakerMatch(seasonId);
            if (firstPlaceTeam.PointTotal == secondPlaceTeam.PointTotal && firstPlaceTeam.GoalAvarageTotal == secondPlaceTeam.GoalAvarageTotal)
            {
                // Head-to-Head Matches
                var headToHeadMatches = await _matchRepository.GetMatchesBySeasonIdAndTeamIds(seasonId, firstPlaceTeam.TeamId, secondPlaceTeam.TeamId);
                if (headToHeadMatches == null || headToHeadMatches.Count() < 2)
                    return BaseResponse<string>.Fail($"Invalid Head to Head matches for teams. FirstTeamId: '{firstPlaceTeam.TeamId}', SecondTeamId: {secondPlaceTeam.TeamId}", 500);

                var firstMatch = headToHeadMatches.First();
                var secondMatch = headToHeadMatches.Last();

                int firstPlaceTeamHomeGoals = headToHeadMatches.FirstOrDefault(match => match.HomeTeamId == firstPlaceTeam.TeamId)?.HomeTeamFullTimeScore ?? 0;
                int firstPlaceTeamAwayGoals = headToHeadMatches.FirstOrDefault(match => match.AwayTeamId == firstPlaceTeam.TeamId)?.AwayTeamFullTimeScore ?? 0;
                int secondPlaceTeamHomeGoals = headToHeadMatches.FirstOrDefault(match => match.HomeTeamId == secondPlaceTeam.TeamId)?.HomeTeamFullTimeScore ?? 0;
                int secondPlaceTeamAwayGoals = headToHeadMatches.FirstOrDefault(match => match.AwayTeamId == secondPlaceTeam.TeamId)?.AwayTeamFullTimeScore ?? 0;

                // Check Head-to-Head Scores 

                if ((firstPlaceTeamHomeGoals + firstPlaceTeamAwayGoals) > (secondPlaceTeamHomeGoals + secondPlaceTeamAwayGoals))
                {
                    placementList.FirstOrDefault(team => team.Id == firstPlaceTeam.TeamId).SeasonPlacement = 1;
                    placementList.FirstOrDefault(team => team.Id == secondPlaceTeam.TeamId).SeasonPlacement = 2;
                }
                else if ((firstPlaceTeamHomeGoals + firstPlaceTeamAwayGoals) < (secondPlaceTeamHomeGoals + secondPlaceTeamAwayGoals))
                {
                    placementList.FirstOrDefault(team => team.Id == secondPlaceTeam.TeamId).SeasonPlacement = 1;
                    placementList.FirstOrDefault(team => team.Id == firstPlaceTeam.TeamId).SeasonPlacement = 2;
                }
                else
                {
                    // Check Head-to-Head Away Goals
                    if (firstPlaceTeamAwayGoals > secondPlaceTeamAwayGoals)
                    {
                        placementList.FirstOrDefault(team => team.Id == firstPlaceTeam.TeamId).SeasonPlacement = 1;
                        placementList.FirstOrDefault(team => team.Id == secondPlaceTeam.TeamId).SeasonPlacement = 2;
                    }
                    else if (secondPlaceTeamAwayGoals > firstPlaceTeamAwayGoals)
                    {
                        placementList.FirstOrDefault(team => team.Id == secondPlaceTeam.TeamId).SeasonPlacement = 1;
                        placementList.FirstOrDefault(team => team.Id == firstPlaceTeam.TeamId).SeasonPlacement = 2;
                    }
                    else
                    {
                        // TODO - If I delete the same repository call above, this one gives null exception. If I leave both of them, this also work. Why?
                        //var tiebreakerMatch = await _matchRepository.GetSeasonTieBreakerMatch(seasonId);

                        // If Tiebreaker match has already been played, Get the result
                        if (tiebreakerMatch == null)
                        {
                            MatchCommand newMatch = new();
                            newMatch.MatchType = Domain.Enums.MatchType.GoldenGoal;
                            newMatch.MatchSeasonPeriodType = MatchSeasonPeriodType.Tiebreaker;
                            newMatch.SeasonRound = ((halfSeasonWeekCount * 2) + 1);
                            newMatch.DatePlanned = DateTime.UtcNow.AddHours(_seasonsSettings.Value.TiebreakerMatchHoursAfter ?? 1);
                            newMatch.SeasonId = seasonId;
                            newMatch.HomeTeamManagerId = firstPlaceTeam.ManagerId;
                            newMatch.HomeTeamId = firstPlaceTeam.TeamId;
                            newMatch.AwayTeamManagerId = secondPlaceTeam.ManagerId;
                            newMatch.AwayTeamId = secondPlaceTeam.TeamId;

                            var createdMatch = await _matchRepository.CreateMatch(newMatch);

                            if (createdMatch == null)
                                return BaseResponse<string>.Fail($"Top 2 teams are tied, CreateTiebreakerMatch failed. FirstTeamId : '{firstPlaceTeam.TeamId}', SecondTeamId: '{secondPlaceTeam.TeamId}'", 500);

                            return BaseResponse<string>.Success($"Top 2 teams are tied, Tiebreaker match created: '{JsonConvert.SerializeObject(createdMatch)}'", 200);
                        }
                        else if (tiebreakerMatch.Winner != null)
                        {
                            if (tiebreakerMatch.Winner == MatchResultType.HomeTeamWin)
                            {
                                placementList.FirstOrDefault(team => team.Id == tiebreakerMatch.HomeTeamId).SeasonPlacement = 1;
                                placementList.FirstOrDefault(team => team.Id == tiebreakerMatch.AwayTeamId).SeasonPlacement = 2;
                            }
                            else
                            {
                                placementList.FirstOrDefault(team => team.Id == tiebreakerMatch.AwayTeamId).SeasonPlacement = 1;
                                placementList.FirstOrDefault(team => team.Id == tiebreakerMatch.HomeTeamId).SeasonPlacement = 2;
                            }
                        }
                        else
                            return BaseResponse<string>.Fail($"Tiebreaker match haven't played yet. MatchTime: {tiebreakerMatch.DatePlanned?.AddHours(_projectSettings.Value.TimezoneDifference ?? 0).ToString("dd/MM/yyyy HH:mm")}", 500);
                    }
                }
            }
            var updateSeasonPlacementResult = await _teamRepository.UpdateSeasonPlacements(placementList);

            if (!updateSeasonPlacementResult)
                return BaseResponse<string>.Fail("Failed to Update Season Placements", 500);

            try
            {
                var updateSeasonChampionResult = await _seasonRepository.UpdateSeasonChampion(seasonId, placementList.First().Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"SeasonService-ConcludeLeague-UpdateSeasonChampion failed. SeasonId: '{season.Id}', ErrorMessage: '{ex.Message}'.");
                placementList.ForEach(team => team.SeasonPlacement = null);
                await _teamRepository.UpdateSeasonPlacements(placementList);
                return BaseResponse<string>.Fail("UpdateSeasonChampion Failed.", 500);
            }

            return BaseResponse<string>.Success("Conclude League Succeeded.", 200);
        }

        public async Task<BaseResponse<bool>> ScheduleTournamentSeason(ScheduleSeasonRequest scheduleSeasonRequest)
        {
            scheduleSeasonRequest.SeasonId ??= _seasonsSettings.Value.CurrentSeasonId ?? await _seasonRepository.GetLastSeasonId();
            scheduleSeasonRequest.SeasonType ??= SeasonType.Tournament;

            var season = await _seasonRepository.GetSeasonById(scheduleSeasonRequest.SeasonId ?? 0);
            if (season == null)
                return BaseResponse<bool>.Fail($"Failed to find Season: {scheduleSeasonRequest.SeasonId}.", 400);

            var matches = await _matchRepository.GetMatchesBySeasonId(scheduleSeasonRequest.SeasonId ?? season.Id);
            if (matches != null && matches.Any())
                return BaseResponse<bool>.Fail($"There are already Matches for this Season: {scheduleSeasonRequest.SeasonId}.", 400);

            var teams = (await _teamRepository.GetTeamsBySeasonId(scheduleSeasonRequest.SeasonId ?? 0))?.ToList();
            if (teams == null || !teams.Any())
                return BaseResponse<bool>.Fail($"Failed to find any teams for Season: {scheduleSeasonRequest.SeasonId}.", 400);

            if (teams.Count < 2)
                return BaseResponse<bool>.Fail($"Can't start a Season with less than 2 teams: {JsonConvert.SerializeObject(teams)}.", 400);

            // Tournament Format - Teams of Power of 2

            var logOfTwo = Math.Log(teams.Count, 2);
            if (logOfTwo % 1 != 0)
                return BaseResponse<bool>.Fail($"Tournament Teams count needs to be a Power of 2. Currently: {teams.Count}. Add '{teams.Count - Math.Pow(2, Math.Floor(logOfTwo))}' or Remove '{Math.Pow(2, Math.Floor(logOfTwo)) - teams.Count}' teams.", 400);

            // Validate Match Times

            if (scheduleSeasonRequest.DailyMatchHours == null || scheduleSeasonRequest.DailyMatchHours.Count == 0)
                scheduleSeasonRequest.DailyMatchHours = _seasonsSettings.Value.DefaultMatchTimes;

            // HH:MM:SS Regex (Seconds are optional) => ([0-1]?[0-9]|[2][0-3]):([0-5][0-9])(:[0-5][0-9])?$
            // HH:MM Regex => ([0-1]?[0-9]|[2][0-3]):([0-5][0-9])$
            Regex hourRegex = new(@"^([0-1]?[0-9]|[2][0-3]):([0-5][0-9])$");
            if (!scheduleSeasonRequest.DailyMatchHours.All(time => hourRegex.Match(time).Success))
                return BaseResponse<bool>.Fail($"Match times need to be in following format: HH:MM", 400);

            // Adjust Match Times to TimeDifference
            if (_projectSettings.Value.TimezoneDifference != null && _projectSettings.Value.TimezoneDifference != 0)
            {
                for (var a = 0; a < scheduleSeasonRequest.DailyMatchHours.Count; a++)
                {
                    var hour = Int32.Parse(scheduleSeasonRequest.DailyMatchHours[a].Split(':')[0]);
                    var minute = Int32.Parse(scheduleSeasonRequest.DailyMatchHours[a].Split(':')[1]);
                    hour -= _projectSettings.Value.TimezoneDifference ?? 0;

                    if (hour < 0)
                        hour += 24;
                    else if (hour > 24)
                        hour -= 24;

                    scheduleSeasonRequest.DailyMatchHours[a] = $"{hour:00}:{minute:00}";
                }
            }

            scheduleSeasonRequest.DailyMatchHours = scheduleSeasonRequest.DailyMatchHours.Order().ToList();

            // Generate Matches By Season Half

            List<BulkInsertMatch> allMatches = new();
            List<int> teamIdList = new();
            teams.ForEach(team => teamIdList.Add(team.Id));
            Random random = new();

            // Generate First Round of Tournament

            while (teamIdList.Count > 0)
            {
                var homeTeamId = teamIdList[random.Next(0, teamIdList.Count)];
                teamIdList.Remove(homeTeamId);
                var awayTeamId = teamIdList[random.Next(0, teamIdList.Count)];
                teamIdList.Remove(awayTeamId);

                var homeTeam = teams.FirstOrDefault(team => team.Id == homeTeamId);
                var awayTeam = teams.FirstOrDefault(team => team.Id == awayTeamId);

                BulkInsertMatch newMatch = new();
                newMatch.MatchType = scheduleSeasonRequest.SeasonType switch
                {
                    SeasonType.League => Domain.Enums.MatchType.League,
                    SeasonType.Tournament => Domain.Enums.MatchType.Tournament,
                    _ => throw new Exception("Invalid Season Type")
                };
                newMatch.MatchSeasonPeriodType = MatchSeasonPeriodType.Tournament;
                newMatch.SeasonRound = teams.Count;
                newMatch.DatePlanned = null;
                newMatch.SeasonId = season.Id;
                newMatch.HomeTeamManagerId = homeTeam?.ManagerId;
                newMatch.HomeTeamId = homeTeamId;
                newMatch.AwayTeamManagerId = awayTeam?.ManagerId;
                newMatch.AwayTeamId = awayTeamId;

                allMatches.Add(newMatch);
            }

            // Generate Match Dates

            DateTime matchDate = scheduleSeasonRequest.FirstMatchAfter == null ? DateTime.UtcNow : ((DateTime)scheduleSeasonRequest.FirstMatchAfter?.AddHours((_projectSettings.Value.TimezoneDifference ?? 0) * -1));
            var newMatchDate = matchDate;

            while (allMatches.Any(match => match.DatePlanned == null))
            {
                foreach (var time in scheduleSeasonRequest.DailyMatchHours)
                {
                    var hour = Int32.Parse(time.Split(':')[0]);
                    var minute = Int32.Parse(time.Split(':')[1]);
                    newMatchDate = new DateTime(matchDate.Year, matchDate.Month, matchDate.Day, hour, minute, 0);

                    if (newMatchDate > matchDate)
                    {
                        var match = allMatches.FirstOrDefault(match => match.DatePlanned == null);
                        if (match != null)
                            match.DatePlanned = newMatchDate;
                    }
                }
                newMatchDate = newMatchDate.AddMinutes(-1).AddDays(1); // Neutralizes this
                matchDate = new DateTime(newMatchDate.Year, newMatchDate.Month, newMatchDate.Day, 0, 0, 0);
            }

            // Create Matches

            var createMatchesResult = await _matchEfcRepository.BulkInsertMatchesForSeason(allMatches);

            if (!createMatchesResult)
                return BaseResponse<bool>.Fail($"Failed to create Matches for Season: {scheduleSeasonRequest.SeasonId}. Team List: {JsonConvert.SerializeObject(allMatches)}", 400);

            try { await _seasonRepository.CloseSeasonRegistration(scheduleSeasonRequest.SeasonId ?? 0, (int?)scheduleSeasonRequest.SeasonType.Value ?? 0); }
            catch (Exception ex)
            {
                _logger.LogError($"SeasonService-ScheduleLeagueSeason-CloseSeasonRegistration failed. SeasonId: '{scheduleSeasonRequest.SeasonId}', SeasonType: '{scheduleSeasonRequest.SeasonType.Value}', ErrorMessage: '{ex.Message}'.");
                await _matchRepository.DeleteMatchesForSeason(season.Id);
                return BaseResponse<bool>.Fail($"Failed to CloseSeasonRegistration for Season: {scheduleSeasonRequest.SeasonId}. Team List: {JsonConvert.SerializeObject(allMatches)}", 500);
            }

            return BaseResponse<bool>.Success(true, 200);
        }

        public async Task<BaseResponse<string>> ScheduleNextRoundOfTournament(int seasonId)
        {
            var tournamentMatches = (await _matchRepository.GetMatchesBySeasonId(seasonId)).OrderByDescending(match => match?.SeasonRound).ToList();

            if (tournamentMatches.Count == 0)
                return BaseResponse<string>.Fail($"ScheduleNextRoundOfTournament fail, no Matches found for SeasonId: {seasonId}.", 404);

            if (tournamentMatches.Any(match => match.Winner is null))
                return BaseResponse<string>.Fail($"ScheduleNextRoundOfTournament fail, there are Matches still haven't played yet for SeasonId: {seasonId}.", 404);

            int lastRound = tournamentMatches.FirstOrDefault()?.SeasonRound ?? 0;

            if (lastRound == 0)
                return BaseResponse<string>.Fail($"ScheduleNextRoundOfTournament fail, can't find the Last SeasonRound Played SeasonId: {seasonId}.", 404);
            else if (lastRound == 2)
                return await ConcludeTournament(seasonId);

            List<int> lastRoundWinners = tournamentMatches.Where(match => match.SeasonRound == lastRound).Select(match =>
            {
                if (match.Winner == MatchResultType.HomeTeamWin)
                    return match.HomeTeamId;
                else if (match.Winner == MatchResultType.AwayTeamWin)
                    return match.AwayTeamId;
                throw new Exception("Tournament matches can't end in a draw.");
            }).ToList();

            // TODO - SCHEDULE NEXT TOURNAMENT ROUND


            return null;
        }

        public async Task<BaseResponse<string>> ConcludeTournament(int seasonId)
        {
            var finalMatch = await _matchRepository.GetFinalMatchOfTheTournament(seasonId);

            if (finalMatch == null)
                return BaseResponse<string>.Fail($"Season not found. SeasonId: {seasonId}", 404);
            else if (finalMatch.Winner == null)
                return BaseResponse<string>.Fail($"Final Match hasn't been played yet. SeasonId: {seasonId}, MatchId: {finalMatch.Id}", 404);

            int winnerId = finalMatch.Winner == MatchResultType.HomeTeamWin ? finalMatch.HomeTeamId : finalMatch.AwayTeamId;

            try
            {
                var updateSeasonChampionResult = await _seasonRepository.UpdateSeasonChampion(seasonId, winnerId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"SeasonService-ConcludeTournament-UpdateSeasonChampion failed. SeasonId: '{seasonId}', ErrorMessage: '{ex.Message}'.");
                return BaseResponse<string>.Fail("UpdateSeasonChampion Failed.", 500);
            }

            return BaseResponse<string>.Success("Conclude Tournament Succeeded.", 200);
        }

        public async Task<BaseResponse<string>> ProceedTournament(int seasonId)
        {
            var tournamentMatches = (await _matchRepository.GetMatchesBySeasonId(seasonId)).OrderByDescending(match => match?.SeasonRound).ToList();

            if (tournamentMatches.Count == 0)
                return BaseResponse<string>.Fail($"ProceedOrConcludeTournament fail, no Matches found for SeasonId: {seasonId}.", 404);

            if (tournamentMatches.Any(match => match.Winner is null))
                return BaseResponse<string>.Fail($"ProceedOrConcludeTournament fail, there are Matches still haven't played yet for SeasonId: {seasonId}.", 404);

            var finalMatch = tournamentMatches.FirstOrDefault(match => match.SeasonRound == 2);

            if (finalMatch != null)
            {
                if (finalMatch.Winner == null)
                    BaseResponse<string>.Fail($"ProceedOrConcludeTournament fail, final Match haven't played yet SeasonId: {seasonId}, Final MatchId: {finalMatch.Id}.", 500); ;

                return await ConcludeTournament(seasonId);
            }
            return await ScheduleNextRoundOfTournament(seasonId);
        }
    }
}
