using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Models.Dtos;
using FootballManager.Application.Models.Requests;
using FootballManager.Application.Models.View;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;
using Microsoft.Extensions.Options;

namespace FootballManager.Persistence.Repositories.Dapper
{
    public class SeasonRepository : BaseRepository, ISeasonRepository
    {
        public SeasonRepository(IOptions<MsSqlSettings> msSqlSettings) : base(msSqlSettings)
        { }


        public async Task<Season> CreateSeason(CreateSeasonRequest createSeasonRequest)
        {
            string createSeasonQuery = @"INSERT INTO Seasons (Name, IsRegistrationOpen, HasSeasonEnded, SeasonType) 
                                         OUTPUT INSERTED.* 
                                         VALUES (@Name, 1, 0, @SeasonType)";
            return await QuerySingleAsync<Season>(createSeasonQuery, createSeasonRequest);
        }

        public async Task<Season> GetSeasonById(int seasonId)
        {
            string getSeasonByIdQuery = @"SELECT * FROM Seasons WITH(NOLOCK) WHERE Id=@SeasonId";
            return await GetAsync<Season>(getSeasonByIdQuery, new { SeasonId = seasonId });
        }

        public async Task<bool> CancelSeasonSchedule(int seasonId)
        {
            try
            {
                string deleteGoalsQuery = @"DELETE FROM Goals WHERE MatchId in (SELECT Id FROM Matches WHERE SeasonId=@SeasonId)";
                await ExecuteAsync(deleteGoalsQuery, new { SeasonId = seasonId });

                string deleteMatchesQuery = @"DELETE FROM Matches WHERE SeasonId=@SeasonId";
                await ExecuteAsync(deleteMatchesQuery, new { SeasonId = seasonId });

                string updateSeasonQuery = @"UPDATE Seasons SET IsRegistrationOpen=1 WHERE Id=@SeasonId";
                return await ExecuteAsync(updateSeasonQuery, new { SeasonId = seasonId }) > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> CloseSeasonRegistration(int seasonId)
        {
            string closeSeasonRegistrationQuery = @"UPDATE Seasons SET IsRegistrationOpen=0 WHERE Id=@SeasonId";
            return await ExecuteAsync(closeSeasonRegistrationQuery, new { SeasonId = seasonId }) > 0;
        }

        public async Task<bool> CloseSeason(int seasonId)
        {
            string closeSeasonQuery = @"UPDATE Seasons SET HasSeasonEnded=1 WHERE Id=@SeasonId";
            return await ExecuteAsync(closeSeasonQuery, new { SeasonId = seasonId }) > 0;
        }

        public async Task<bool> DeleteSeasonById(int seasonId)
        {
            string deleteSeasonByIdQuery = @"DELETE FROM Seasons WHERE Id=@SeasonId";
            return await ExecuteAsync(deleteSeasonByIdQuery, new { SeasonId = seasonId }) > 0;
        }

        public async Task<bool> UpdateSeasonType(int seasonId, int seasonType)
        {
            string updateSeasonTypeQuery = @"UPDATE Seasons SET SeasonType=@SeasonType WHERE Id=@SeasonId";
            return await ExecuteAsync(updateSeasonTypeQuery, new { SeasonId = seasonId, SeasonType = seasonType }) > 0;
        }

        public async Task<bool> UpdateSeasonSettingsId(int seasonId, int seasonSettingsId)
        {
            string updateSeasonSettingsIdQuery = @"UPDATE Seasons SET SeasonSettingsId=@SeasonSettingsId WHERE Id=@SeasonId";
            return await ExecuteAsync(updateSeasonSettingsIdQuery, new { SeasonId = seasonId, SeasonSettingsId = seasonSettingsId }) > 0;
        }

        public async Task<bool> CloseSeasonRegistration(int seasonId, int seasonType)
        {
            string closeSeasonRegistrationQuery = @"UPDATE Seasons SET SeasonType=@SeasonType, IsRegistrationOpen=0 WHERE Id=@SeasonId";
            return await ExecuteAsync(closeSeasonRegistrationQuery, new { SeasonId = seasonId, SeasonType = seasonType }) > 0;
        }

        public async Task<IEnumerable<SeasonTeamStandings>> GetSeasonTeamStandings(int seasonId)
        {
            string getSeasonTeamStandingsQuery = $@"SELECT season.Name AS 'SeasonName', team.Id AS 'TeamId', team.Name AS 'TeamName', team.SeasonPlacement AS 'SeasonPlacement', team.ManagerId AS 'ManagerId', manager.Name AS 'ManagerName', 
                                                    (ISNULL(homematches.MatchesPlayed, 0) + ISNULL(awaymatches.MatchesPlayed, 0)) AS 'MatchesPlayedTotal', (ISNULL(homematches.PointTotal, 0) + ISNULL(awaymatches.PointTotal, 0)) AS 'PointTotal', 
                                                    ((ISNULL(homematches.FullTimeScoreSum, 0) + ISNULL(homematches.OvertimeScoreSum, 0)) + (ISNULL(awaymatches.FullTimeScoreSum, 0) + ISNULL(awaymatches.OvertimeScoreSum, 0))) AS 'ScoredTotal', 
                                                    ((ISNULL(homematches.FullTimeConcededSum, 0) + ISNULL(homematches.OvertimeConcededSum, 0)) + (ISNULL(awaymatches.FullTimeConcededSum, 0) + ISNULL(awaymatches.OvertimeConcededSum, 0))) AS 'ConcededTotal', 
                                                    (ISNULL(homematches.MatchesWon, 0) + ISNULL(awaymatches.MatchesWon, 0)) AS 'MatchesWonTotal', (ISNULL(homematches.MatchesDraw, 0) + ISNULL(awaymatches.MatchesDraw, 0)) AS 'MatchesDrawTotal', (ISNULL(homematches.MatchesLost, 0) + ISNULL(awaymatches.MatchesLost, 0)) AS 'MatchesLostTotal', 
                                                    (((ISNULL(homematches.FullTimeScoreSum, 0) + ISNULL(homematches.OvertimeScoreSum, 0)) - (ISNULL(homematches.FullTimeConcededSum, 0) + ISNULL(homematches.OvertimeConcededSum, 0))) + ((ISNULL(awaymatches.FullTimeScoreSum, 0) + ISNULL(awaymatches.OvertimeScoreSum, 0)) - (ISNULL(awaymatches.FullTimeConcededSum, 0) + ISNULL(awaymatches.OvertimeConcededSum, 0)))) AS 'GoalAvarageTotal', 
                                                
                                                    ISNULL(homematches.MatchesPlayed, 0) AS 'HomeMatchesPlayed', ISNULL(homematches.PointTotal, 0) AS 'HomePointTotal', 
                                                    ISNULL(homematches.MatchesWon, 0) AS 'HomeMatchesWon', ISNULL(homematches.MatchesDraw, 0) AS 'HomeMatchesDraw', ISNULL(homematches.MatchesLost, 0) AS 'HomeMatchesLost', 
                                                    ISNULL(homematches.FullTimeScoreSum, 0) AS 'HomeFullTimeScoreSum', ISNULL(homematches.OvertimeScoreSum, 0) AS 'HomeOvertimeScoreSum', (ISNULL(homematches.FullTimeScoreSum, 0) + ISNULL(homematches.OvertimeScoreSum, 0)) AS 'HomeScoreSum', 
                                                    ISNULL(homematches.FullTimeConcededSum, 0) AS 'HomeFullTimeConcededSum', ISNULL(homematches.OvertimeConcededSum, 0) AS 'HomeOvertimeConcededSum', (ISNULL(homematches.FullTimeConcededSum, 0) + ISNULL(homematches.OvertimeConcededSum, 0)) AS 'HomeConcededSum', 
                                                    ((ISNULL(homematches.FullTimeScoreSum, 0) + ISNULL(homematches.OvertimeScoreSum, 0)) - (ISNULL(homematches.FullTimeConcededSum, 0) + ISNULL(homematches.OvertimeConcededSum, 0))) AS 'HomeGoalAvarage', 

                                                    ISNULL(awaymatches.MatchesPlayed, 0) AS 'AwayMatchesPlayed', ISNULL(awaymatches.PointTotal, 0) AS 'AwayPointTotal', 
                                                    ISNULL(awaymatches.MatchesWon, 0) AS 'AwayMatchesWon', ISNULL(awaymatches.MatchesDraw, 0) AS 'AwayMatchesDraw', ISNULL(awaymatches.MatchesLost, 0) AS 'AwayMatchesLost', 
                                                    ISNULL(awaymatches.FullTimeScoreSum, 0) AS 'AwayFullTimeScoreSum', ISNULL(awaymatches.OvertimeScoreSum, 0) AS 'AwayOvertimeScoreSum', (ISNULL(awaymatches.FullTimeScoreSum, 0) + ISNULL(awaymatches.OvertimeScoreSum, 0)) AS 'AwayScoreSum', 
                                                    ISNULL(awaymatches.FullTimeConcededSum, 0) AS 'AwayFullTimeConcededSum', ISNULL(awaymatches.OvertimeConcededSum, 0) AS 'AwayOvertimeConcededSum', (ISNULL(awaymatches.FullTimeConcededSum, 0) + ISNULL(awaymatches.OvertimeConcededSum, 0)) AS 'AwayConcededSum', 
                                                    ((ISNULL(awaymatches.FullTimeScoreSum, 0) + ISNULL(awaymatches.OvertimeScoreSum, 0)) - (ISNULL(awaymatches.FullTimeConcededSum, 0) + ISNULL(awaymatches.OvertimeConcededSum, 0))) AS 'AwayGoalAvarage' 

                                                    FROM Seasons AS season WITH(NOLOCK) 
                                                    LEFT JOIN Teams AS team WITH(NOLOCK) ON team.SeasonId=season.Id 
                                                    LEFT JOIN Users AS manager WITH(NOLOCK) ON manager.Id=team.ManagerId 

                                                    OUTER APPLY (SELECT COUNT(*) AS 'MatchesPlayed', SUM(CASE WHEN Winner=1 THEN 3 WHEN Winner=0 THEN 1 ELSE 0 END) AS 'PointTotal', 
                                                    SUM(CASE WHEN Winner=1 THEN 1 ELSE 0 END) AS 'MatchesWon', SUM(CASE WHEN Winner=0 THEN 1 ELSE 0 END) AS 'MatchesDraw', SUM(CASE WHEN Winner=2 THEN 1 ELSE 0 END) AS 'MatchesLost', 
                                                    SUM(ISNULL(HomeTeamFullTimeScore, 0)) AS 'FullTimeScoreSum', SUM(ISNULL(AwayTeamFullTimeScore, 0)) AS 'FullTimeConcededSum', 
                                                    SUM(ISNULL(HomeTeamOvertimeScore, 0)) AS 'OvertimeScoreSum', SUM(ISNULL(AwayTeamOvertimeScore, 0)) AS 'OvertimeConcededSum' 
                                                    FROM Matches as home WITH(NOLOCK) WHERE home.HomeTeamId=team.Id AND Winner IS NOT NULL GROUP BY home.HomeTeamId) AS homematches 

                                                    OUTER APPLY (SELECT COUNT(*) AS 'MatchesPlayed', SUM(CASE WHEN Winner=2 THEN 3 WHEN Winner=0 THEN 1 ELSE 0 END) AS 'PointTotal', 
                                                    SUM(CASE WHEN Winner=2 THEN 1 ELSE 0 END) AS 'MatchesWon', SUM(CASE WHEN Winner=0 THEN 1 ELSE 0 END) AS 'MatchesDraw', SUM(CASE WHEN Winner=1 THEN 1 ELSE 0 END) AS 'MatchesLost', 
                                                    SUM(ISNULL(AwayTeamFullTimeScore, 0)) AS 'FullTimeScoreSum', SUM(ISNULL(HomeTeamFullTimeScore, 0)) AS 'FullTimeConcededSum', 
                                                    SUM(ISNULL(AwayTeamOvertimeScore, 0)) AS 'OvertimeScoreSum', SUM(ISNULL(HomeTeamOvertimeScore, 0)) AS 'OvertimeConcededSum' 
                                                    FROM Matches AS away WITH(NOLOCK) WHERE away.AwayTeamId=team.Id AND Winner IS NOT NULL GROUP BY away.AwayTeamId) AS awaymatches 

                                                    WHERE season.Id=@SeasonId";

            return await GetAllAsync<SeasonTeamStandings>(getSeasonTeamStandingsQuery, new { SeasonId = seasonId });
        }

        public async Task<IEnumerable<SeasonPlayerStandings>> GetSeasonPlayerStandings(int seasonId)
        {
            string getSeasonPlayerStandingsQuery = $@"SELECT player.Id AS 'PlayerId', player.Name as 'PlayerName', player.Position AS 'PlayerPosition', player.Number AS 'PlayerNumber', team.Id as 'TeamId', team.Name as 'TeamName', 
                                                      ISNULL(matches.MatchesPlayed, 0) AS 'MatchesPlayed', ISNULL(goalsscored.ScoredSum, 0) AS 'GoalsScored', ISNULL(goalsassisted.AssistedSum, 0) AS 'GoalsAssisted', (ISNULL(goalsscored.ScoredSum, 0) + ISNULL(goalsassisted.AssistedSum, 0)) AS 'GoalTotal', season.Name AS 'SeasonName' 
                                                      FROM Seasons AS season WITH(NOLOCK) 
                                                      LEFT JOIN Teams AS team WITH(NOLOCK) ON team.SeasonId=season.Id 
                                                      LEFT JOIN Players AS player WITH(NOLOCK) ON player.TeamId=team.Id 
                                                      OUTER APPLY (SELECT COUNT(*) AS 'MatchesPlayed' FROM Matches AS matches WITH(NOLOCK) WHERE (matches.HomeTeamId=team.Id AND matches.Winner IS NOT NULL) OR (matches.AwayTeamId=team.Id AND matches.Winner IS NOT NULL)) as matches 
                                                      OUTER APPLY (SELECT COUNT(*) AS 'ScoredSum' FROM Goals AS goalscored WITH(NOLOCK) WHERE goalscored.ScoredById=player.Id) AS goalsscored 
                                                      OUTER APPLY (SELECT COUNT(*) AS 'AssistedSum' FROM Goals AS goalscored WITH(NOLOCK) WHERE goalscored.AssistedById=player.Id) AS goalsassisted 
                                                      WHERE season.Id=@SeasonId";

            return await GetAllAsync<SeasonPlayerStandings>(getSeasonPlayerStandingsQuery, new { SeasonId = seasonId });
        }

        public async Task<IEnumerable<SeasonMatchFixture>> GetSeasonMatchFixture(int seasonId)
        {
            string getSeasonStandingsQuery = $@"SELECT season.Name AS 'SeasonName', matches.Id AS 'MatchId', matches.MatchType AS 'MatchType', matches.MatchSeasonPeriodType AS 'MatchSeasonPeriodType', 
                                                matches.SeasonRound AS 'SeasonRound', matches.DatePlanned AS 'MatchDatePlanned', matches.DatePlayed AS 'MatchDatePlayed', matches.Winner AS 'MatchWinner', 
                                                matches.HomeTeamId AS 'HomeTeamId', hometeam.HomeTeamName AS 'HomeTeamName', ISNULL(matches.HomeTeamFullTimeScore, 0) AS 'HomeTeamFullTimeScore', matches.HomeTeamOvertimeScore AS 'HomeTeamOvertimeScore', matches.HomeTeamPenaltyScore AS 'HomeTeamPenaltyScore', (ISNULL(matches.HomeTeamFullTimeScore, 0) + ISNULL(matches.HomeTeamOvertimeScore, 0)) AS 'HomeTeamFinalScore', 
                                                matches.AwayTeamId AS 'AwayTeamId', awayteam.AwayTeamName AS 'AwayTeamName', ISNULL(matches.AwayTeamFullTimeScore, 0) AS 'AwayTeamFullTimeScore', matches.AwayTeamOvertimeScore AS 'AwayTeamOvertimeScore', matches.AwayTeamPenaltyScore AS 'AwayTeamPenaltyScore', (ISNULL(matches.AwayTeamFullTimeScore, 0) + ISNULL(matches.AwayTeamOvertimeScore, 0)) AS 'AwayTeamFinalScore', 
                                                matches.HomeTeamManagerId AS 'HomeTeamManagerId', homemanager.HomeTeamManagerName AS 'HomeTeamManagerName', matches.AwayTeamManagerId AS 'AwayTeamManagerId', awaymanager.AwayTeamManagerName AS 'AwayTeamManagerName', 
                                                goal.MinuteScored AS 'GoalMinuteScored', (CASE WHEN goal.TeamScoredId = matches.HomeTeamId THEN 1 ELSE 2 END) AS 'MatchSideScored', 
                                                scoredby.GoalScoredById AS 'GoalScoredById', scoredby.GoalScoredByName AS 'GoalScoredByName', scoredby.GoalScoredByNumber AS 'GoalScoredByNumber', 
                                                assistby.GoalAssistById AS 'GoalAssistById', assistby.GoalAssistByName AS 'GoalAssistByName', assistby.GoalAssistByNumber AS 'GoalAssistByNumber' 
                                                FROM Matches AS matches WITH(NOLOCK) 
                                                LEFT JOIN Seasons AS season WITH(NOLOCK) ON season.Id=matches.SeasonId 
                                                LEFT JOIN Goals AS goal WITH(NOLOCK) ON goal.MatchId=matches.Id 
                                                OUTER APPLY (SELECT homemanager.Name AS 'HomeTeamManagerName' FROM Users AS homemanager WITH(NOLOCK) WHERE homemanager.Id=matches.HomeTeamManagerId) AS homemanager 
                                                OUTER APPLY (SELECT awaymanager.Name AS 'AwayTeamManagerName' FROM Users AS awaymanager WITH(NOLOCK) WHERE awaymanager.Id=matches.AwayTeamManagerId) AS awaymanager 
                                                OUTER APPLY (SELECT hometeam.Name AS 'HomeTeamName' FROM Teams AS hometeam WITH(NOLOCK) WHERE hometeam.Id=matches.HomeTeamId) AS hometeam 
                                                OUTER APPLY (SELECT awayteam.Name AS 'AwayTeamName' FROM Teams AS awayteam WITH(NOLOCK) WHERE awayteam.Id=matches.AwayTeamId) AS awayteam 
                                                OUTER APPLY (SELECT goal.MinuteScored AS 'GoalMinuteScored', scoredby.Id AS 'GoalScoredById', scoredby.Name AS 'GoalScoredByName', scoredby.Number AS 'GoalScoredByNumber' FROM Players AS scoredby WITH(NOLOCK) WHERE scoredby.Id=goal.ScoredById) AS scoredby 
                                                OUTER APPLY (SELECT assistby.Id AS 'GoalAssistById', assistby.Name AS 'GoalAssistByName', assistby.Number AS 'GoalAssistByNumber'  FROM Players AS assistby WITH(NOLOCK) WHERE assistby.Id=goal.AssistedById) AS assistby 
                                                WHERE matches.SeasonId=@SeasonId";

            return await GetAllAsync<SeasonMatchFixture>(getSeasonStandingsQuery, new { SeasonId = seasonId });
        }

        public async Task<bool> UpdateSeasonChampion(int seasonId, int championId)
        {
            string updateSeasonChampionQuery = @"UPDATE Seasons SET ChampionId=@ChampionId WHERE Id=@SeasonId";

            return await ExecuteAsync(updateSeasonChampionQuery, new { SeasonId = seasonId, ChampionId = championId }) > 0;
        }

        public async Task<int?> GetLastSeasonId()
        {
            string updateSeasonChampionQuery = @"SELECT TOP(1) Id FROM Seasons WITH(NOLOCK) WHERE (Name not like '%(WL)' AND Name not like '%(WT)') ORDER BY ID DESC";

            return await GetAsync<int?>(updateSeasonChampionQuery, null);
        }

        public async Task<int?> GetLastWeeklyLeagueId()
        {
            string updateSeasonChampionQuery = @"SELECT TOP(1) Id FROM Seasons WITH(NOLOCK) WHERE Name like '%(WL)' ORDER BY ID DESC";

            return await GetAsync<int?>(updateSeasonChampionQuery, null);
        }

        public async Task<int?> GetLastWeeklyTournamentId()
        {
            string updateSeasonChampionQuery = @"SELECT TOP(1) Id FROM Seasons WITH(NOLOCK) WHERE Name like '%(WT)' ORDER BY ID DESC";

            return await GetAsync<int?>(updateSeasonChampionQuery, null);
        }

        public async Task<IEnumerable<SeasonsToConclude>> GetLeaguesToConclude()
        {
            string getLeaguesToConcludeQuery = @"SELECT m.SeasonId AS 'SeasonId' FROM Matches AS m WITH(NOLOCK) 
                                                 LEFT JOIN Seasons AS s WITH(NOLOCK) ON s.Id=m.SeasonId 
                                                 WHERE s.ChampionId IS NULL AND s.SeasonType=@SeasonType 
                                                 GROUP BY m.SeasonId 
                                                 HAVING COUNT(m.Id)=COUNT(m.Winner)";

            return await GetAllAsync<SeasonsToConclude>(getLeaguesToConcludeQuery, new { SeasonType = SeasonType.League });
        }

        public async Task<IEnumerable<SeasonsToConclude>> GetTournamentsToConclude()
        {
            string getTournamentsToConcludeQuery = @"SELECT m.SeasonId AS 'SeasonId' FROM Matches AS m WITH(NOLOCK) 
                                                     LEFT JOIN Seasons AS s WITH(NOLOCK) ON s.Id=m.SeasonId 
                                                     WHERE s.ChampionId IS NULL AND s.SeasonType=@SeasonType 
                                                     GROUP BY m.SeasonId 
                                                     HAVING COUNT(CASE WHEN Round=2 AND Winner IS NOT NULL THEN 1 END) > 0";

            return await GetAllAsync<SeasonsToConclude>(getTournamentsToConcludeQuery, new { SeasonType = SeasonType.Tournament });
        }

        public async Task<IEnumerable<TournamentsToProceed>> CheckTournamentsToProceed()
        {
            string checkTournamentsToProceedQuery = @"SELECT m.SeasonId AS 'SeasonId' FROM Matches AS m WITH(NOLOCK) 
                                                      LEFT JOIN Seasons AS s WITH(NOLOCK) ON m.SeasonId=s.Id 
                                                      WHERE s.SeasonType=@SeasonType 
                                                      GROUP BY m.SeasonId, s.SeasonType 
                                                      HAVING COUNT(*)=COUNT(m.Winner)";

            return await GetAllAsync<TournamentsToProceed>(checkTournamentsToProceedQuery, new { SeasonType = SeasonType.Tournament });
        }

        public async Task<Season> GetLastWeeklyLeague()
        {
            string getLastWeeklyLeagueQuery = @"SELECT TOP(1) * FROM Seasons WITH(NOLOCK) WHERE Name like '%' + '(WL)' AND SeasonType=@SeasonType ORDER BY Name DESC";
            return await GetAsync<Season>(getLastWeeklyLeagueQuery, new { SeasonType = SeasonType.League });
        }

        public async Task<Season> GetLastWeeklyTournament()
        {
            string getLastWeeklyTournamentQuery = @"SELECT TOP(1) * FROM Seasons WITH(NOLOCK) WHERE Name like '%' + '(WT)' AND SeasonType=@SeasonType ORDER BY Name DESC";
            return await GetAsync<Season>(getLastWeeklyTournamentQuery, new { SeasonType = SeasonType.Tournament });
        }
    }
}
