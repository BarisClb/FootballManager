using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Dtos;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;
using Microsoft.Extensions.Options;

namespace FootballManager.Persistence.Repositories.Dapper
{
    public class MatchRepository : BaseRepository, IMatchRepository
    {

        public MatchRepository(IOptions<MsSqlSettings> msSqlSettings) : base(msSqlSettings)
        { }


        public async Task<Match> CreateMatch(MatchCommand createMatchCommand)
        {
            string createMatchQuery = @"INSERT INTO Matches (MatchType, MatchSeasonPeriodType, SeasonRound, HomeTeamFullTimeScore, AwayTeamFullTimeScore, HomeTeamOvertimeScore, AwayTeamOvertimeScore, 
                                        HomeTeamPenaltyScore, AwayTeamPenaltyScore, Winner, DatePlanned, DatePlayed, SeasonId, HomeTeamManagerId, HomeTeamId, AwayTeamManagerId, AwayTeamId) 
                                        OUTPUT INSERTED.* 
                                        VALUES (@MatchType, @MatchSeasonPeriodType, @SeasonRound, @HomeTeamFullTimeScore, @AwayTeamFullTimeScore, @HomeTeamOvertimeScore, @AwayTeamOvertimeScore, 
                                        @HomeTeamPenaltyScore, @AwayTeamPenaltyScore, @Winner, @DatePlanned, @DatePlayed, @SeasonId, @HomeTeamManagerId, @HomeTeamId, @AwayTeamManagerId, @AwayTeamId)";
            return await QuerySingleAsync<Match>(createMatchQuery, createMatchCommand);
        }

        public async Task<Match> GetMatchById(int matchId)
        {
            string getMatchByIdQuery = @"SELECT * FROM Matches WITH(NOLOCK) WHERE Id=@MatchId";
            return await GetAsync<Match>(getMatchByIdQuery, new { MatchId = matchId });
        }

        public async Task<IEnumerable<Match>> GetMatchesBySeasonId(int seasonId)
        {
            string getMatchesBySeasonIdQuery = @"SELECT * FROM Matches WITH(NOLOCK) WHERE SeasonId=@SeasonId";
            return await GetAllAsync<Match>(getMatchesBySeasonIdQuery, new { SeasonId = seasonId });
        }

        public async Task<IEnumerable<Match>> GetMatchesToSimulate()
        {
            string getMatchesBySeasonIdQuery = @"SELECT * FROM Matches WITH(NOLOCK) WHERE DatePlanned<DATEADD(MINUTE, 1, GETUTCDATE()) and Winner is null";
            return await GetAllAsync<Match>(getMatchesBySeasonIdQuery, null);
        }

        public async Task<Match> ConcludeMatch(MatchCommand concludeMatchCommand)
        {
            string concludeMatchQuery = @"UPDATE Matches SET HomeTeamFullTimeScore=@HomeTeamFullTimeScore, AwayTeamFullTimeScore=@AwayTeamFullTimeScore, HomeTeamOvertimeScore=@HomeTeamOvertimeScore, AwayTeamOvertimeScore=@AwayTeamOvertimeScore, 
                                          HomeTeamPenaltyScore=@HomeTeamPenaltyScore, AwayTeamPenaltyScore=@AwayTeamPenaltyScore, Winner=@Winner, DatePlayed=GETUTCDATE(), DateUpdated=GETUTCDATE() 
                                          OUTPUT INSERTED.* 
                                          WHERE Id=@Id";
            return await QuerySingleAsync<Match>(concludeMatchQuery, concludeMatchCommand);
        }

        public async Task<bool> DeleteMatchesForSeason(int seasonId)
        {
            string deletMatchesForSeasonQuery = @"DELETE FROM Matches WHERE SeasonId=@SeasonId";
            await ExecuteAsync(deletMatchesForSeasonQuery, new { SeasonId = seasonId });
            return true;
        }

        public async Task<IEnumerable<Match>> GetMatchesBySeasonIdAndTeamIds(int seasonId, int teamOneId, int teamTwoId)
        {
            string getMatchesBySeasonIdAndTeamIdsQuery = @"SELECT * FROM Matches WITH(NOLOCK) WHERE SeasonId=@SeasonId AND ((HomeTeamId=@TeamOneId AND AwayTeamId=@TeamTwoId) OR (HomeTeamId=@TeamTwoId AND AwayTeamId=@TeamOneId))";
            return await GetAllAsync<Match>(getMatchesBySeasonIdAndTeamIdsQuery, new { SeasonId = seasonId, TeamOneId = teamOneId, TeamTwoId = teamTwoId });
        }

        public async Task<Match> GetSeasonTieBreakerMatch(int seasonId)
        {
            string getLastMatchBySeasonPeriodTypeAndTeamIdsQuery = @"SELECT * FROM Matches WITH(NOLOCK) WHERE SeasonId=@SeasonId AND MatchSeasonPeriodType=@MatchSeasonPeriodType";
            return await GetAsync<Match>(getLastMatchBySeasonPeriodTypeAndTeamIdsQuery, new { SeasonId = seasonId, MatchSeasonPeriodType = MatchSeasonPeriodType.Tiebreaker });
        }

        public async Task<Match> GetFinalMatchOfTheTournament(int seasonId)
        {
            string getFinalMatchOfTheTournamentQuery = @"SELECT TOP(1) * FROM Matches WITH(NOLOCK) WHERE SeasonId=@SeasonId AND SeasonRound=@SeasonRound";
            return await GetAsync<Match>(getFinalMatchOfTheTournamentQuery, new { SeasonId = seasonId, SeasonRound = 2 });
        }

        // DapperPlus - Trial Period ends every month, instead of keep Updating the version I implemented EFC for Add-Update-Delete Range

        public async Task<bool> BulkInsertMatchesForSeason(List<BulkInsertMatch> matchList)
        {
            await BulkInsertAsync(matchList);
            return true;
        }
    }
}
