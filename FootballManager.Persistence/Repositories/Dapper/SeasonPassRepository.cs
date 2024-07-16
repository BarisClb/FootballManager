using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Requests;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FootballManager.Persistence.Repositories.Dapper
{
    public class SeasonPassRepository : BaseRepository, ISeasonPassRepository
    {
        public SeasonPassRepository(IOptions<MsSqlSettings> msSqlSettings) : base(msSqlSettings)
        { }


        public async Task<SeasonPass> CreateSeasonPass(CreateSeasonPassRequest createSeasonPassRequest)
        {
            string createSeasonPassQuery = @"INSERT INTO SeasonPasses (SeasonId, Password) 
                                             OUTPUT INSERTED.* 
                                             VALUES (@SeasonId, @Password)";
            return await QuerySingleAsync<SeasonPass>(createSeasonPassQuery, createSeasonPassRequest);
        }

        public async Task<SeasonPass> UseSeasonPass(SeasonPassCommand useSeasonPassCommand)
        {
            string createSeasonPassQuery = @"UPDATE SeasonPasses SET DateUsed=GETUTCDATE(), ManagerId=@ManagerId, TeamId=@TeamId, DateUpdated=GETUTCDATE() 
                                             OUTPUT INSERTED.* 
                                             WHERE Id=@Id";
            return await QuerySingleAsync<SeasonPass>(createSeasonPassQuery, useSeasonPassCommand);
        }

        public async Task<SeasonPass> GetSeasonPassById(int seasonPassId)
        {
            string getSeasonPassById = @"SELECT * FROM SeasonPasses WITH(NOLOCK) WHERE Id=@SeasonPassId";
            return await GetAsync<SeasonPass>(getSeasonPassById, new { SeasonPassId = seasonPassId });
        }

        public async Task<SeasonPass> GetValidSeasonPassBySeasonIdAndPassword(int seasonId, string password)
        {
            string createSeasonPassQuery = @"SELECT * FROM SeasonPasses WITH(NOLOCK) WHERE SeasonId=@SeasonId and Password=@Password and DateUsed is null";
            return await GetAsync<SeasonPass>(createSeasonPassQuery, new { SeasonId = seasonId, Password = password });
        }
    }
}
