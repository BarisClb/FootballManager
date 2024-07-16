using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Dtos;
using FootballManager.Application.Models.View;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FootballManager.Persistence.Repositories.Dapper
{
    public class TeamRepository : BaseRepository, ITeamRepository
    {
        public TeamRepository(IOptions<MsSqlSettings> msSqlSettings) : base(msSqlSettings)
        { }


        public async Task<Team> CreateTeam(TeamCommand createTeamCommand)
        {
            string createTeamQuery = @"INSERT INTO Teams (Name, ManagerId, SeasonId, SeasonPlacement) 
                                       OUTPUT INSERTED.* 
                                       VALUES (@Name, @ManagerId, @SeasonId, @SeasonPlacement)";
            return await QuerySingleAsync<Team>(createTeamQuery, createTeamCommand);
        }

        public async Task<Team> GetTeamById(int teamId)
        {
            string getTeamByIdQuery = @"SELECT * FROM Teams WITH(NOLOCK) WHERE Id=@TeamId";
            return await GetAsync<Team>(getTeamByIdQuery, new { TeamId = teamId });
        }

        public async Task<IEnumerable<Team>> GetTeamsBySeasonId(int seasonId)
        {
            string getTeamsBySeasonId = @"SELECT * FROM Teams WITH(NOLOCK) WHERE SeasonId=@SeasonId";
            return await GetAllAsync<Team>(getTeamsBySeasonId, new { SeasonId = seasonId });
        }

        public async Task<bool> DeleteTeamById(int teamId)
        {
            string deleteTeamById = @"DELETE FROM Teams WHERE Id=@TeamId";
            return await ExecuteAsync(deleteTeamById, new { TeamId = teamId }) > 0;
        }

        public async Task<bool> DeleteTeamsBySeasonId(int seasonId)
        {
            string deleteTeamById = @"DELETE FROM Teams WHERE SeasonId=@SeasonId";
            return await ExecuteAsync(deleteTeamById, new { SeasonId = seasonId }) > 0;
        }

        public async Task<Team> FindTeamBySeasonAndManager(int seasonId, int managerId)
        {
            string findTeamBySeasonAndManagerQuery = @"SELECT * FROM Teams WITH(NOLOCK) WHERE SeasonId=@SeasonId and ManagerId=@ManagerId";
            return await GetAsync<Team>(findTeamBySeasonAndManagerQuery, new { SeasonId = seasonId, ManagerId = managerId });
        }

        public async Task<SeasonTeamStandings> GetTeamStats(int teamId)
        {
            string getTeamStatsQuery = $@"SELECT season.Name AS 'SeasonName', team.Id AS 'TeamId', team.Name AS 'TeamName', team.ManagerId AS 'ManagerId', manager.Name AS 'ManagerName', 
                                          (ISNULL(homematches.MatchesPlayed, 0) + ISNULL(awaymatches.MatchesPlayed, 0)) AS 'MatchesPlayedTotal', (ISNULL(homematches.PointTotal, 0) + ISNULL(awaymatches.PointTotal, 0)) AS 'PointTotal', 
                                          ((ISNULL(homematches.FullTimeScoreSum, 0) + ISNULL(homematches.OvertimeScoreSum, 0)) + (ISNULL(awaymatches.FullTimeScoreSum, 0) + ISNULL(awaymatches.OvertimeScoreSum, 0))) AS 'ScoredTotal', 
                                          ((ISNULL(homematches.FullTimeConcededSum, 0) + ISNULL(homematches.OvertimeConcededSum, 0)) + (ISNULL(awaymatches.FullTimeConcededSum, 0) + ISNULL(awaymatches.OvertimeConcededSum, 0))) AS 'ConcededTotal', 
                                          (ISNULL(homematches.MatchesWon, 0) + ISNULL(awaymatches.MatchesWon, 0)) AS 'MatchesWonTotal', (ISNULL(homematches.MatchesDraw, 0) + ISNULL(awaymatches.MatchesDraw, 0)) AS 'MatchesDrawTotal', (ISNULL(homematches.MatchesLost, 0) + ISNULL (awaymatches.MatchesLost, 0)) AS 'MatchesLostTotal', 
                                          (((ISNULL(homematches.FullTimeScoreSum, 0) + ISNULL(homematches.OvertimeScoreSum, 0)) - (ISNULL(homematches.FullTimeConcededSum, 0) + ISNULL(homematches.OvertimeConcededSum, 0))) + ((ISNULL(awaymatches.FullTimeScoreSum, 0) + ISNULL (awaymatches.OvertimeScoreSum, 0)) - (ISNULL(awaymatches.FullTimeConcededSum, 0) + ISNULL(awaymatches.OvertimeConcededSum, 0)))) AS 'GoalAvarageTotal', 
                                                  
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
                                          FROM Matches as home WITH(NOLOCK) WHERE home.HomeTeamId=team.Id AND Winner is not null GROUP BY home.HomeTeamId) AS homematches 
                                          
                                          OUTER APPLY (SELECT COUNT(*) AS 'MatchesPlayed', SUM(CASE WHEN Winner=2 THEN 3 WHEN Winner=0 THEN 1 ELSE 0 END) AS 'PointTotal', 
                                          SUM(CASE WHEN Winner=2 THEN 1 ELSE 0 END) AS 'MatchesWon', SUM(CASE WHEN Winner=0 THEN 1 ELSE 0 END) AS 'MatchesDraw', SUM(CASE WHEN Winner=1 THEN 1 ELSE 0 END) AS 'MatchesLost', 
                                          SUM(ISNULL(AwayTeamFullTimeScore, 0)) AS 'FullTimeScoreSum', SUM(ISNULL(HomeTeamFullTimeScore, 0)) AS 'FullTimeConcededSum', 
                                          SUM(ISNULL(AwayTeamOvertimeScore, 0)) AS 'OvertimeScoreSum', SUM(ISNULL(HomeTeamOvertimeScore, 0)) AS 'OvertimeConcededSum' 
                                          FROM Matches AS away WITH(NOLOCK) WHERE away.AwayTeamId=team.Id AND Winner is not null GROUP BY away.AwayTeamId) AS awaymatches 

                                          WHERE team.Id=@TeamId";

            return await GetAsync<SeasonTeamStandings>(getTeamStatsQuery, new { TeamId = teamId });
        }

        // DapperPlus - Trial Period ends every month, instead of keep Updating the version I implemented EFC for Add-Update-Delete Range

        public async Task<bool> UpdateSeasonPlacements(List<BulkUpdateSeasonPlacement> teamPlacements)
        {
            await BulkUpdateAsync(teamPlacements);
            return true;
        }
    }
}
