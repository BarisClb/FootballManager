using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballManager.Persistence.Repositories.Dapper
{
    public class SeasonSettingsRepository : BaseRepository, ISeasonSettingsRepository
    {
        public SeasonSettingsRepository(IOptions<MsSqlSettings> msSqlSettings) : base(msSqlSettings)
        { }


        public async Task<SeasonSettings> CreateSeasonSettings(SeasonSettingsCommand createSeasonSettingsCommand)
        {
            string createSeasonSettings = @"INSERT INTO SeasonSettings (SeasonId, MatchTimes, MatchIntervalMinutes, LeagueHalvesMinumumIntervalHours, HomeTeamExtraGoalChance, TiebreakerMatchHoursAfter, StartNextRoundNextDay, AllowNoAssistGoals, NumberOfTeamsToParticipate, SeasonStartDay) 
                                            OUTPUT INSERTED.* 
                                            VALUES (@SeasonId, @MatchTimes, @MatchIntervalMinutes, @LeagueHalvesMinumumIntervalHours, @HomeTeamExtraGoalChance, @TiebreakerMatchHoursAfter, @StartNextRoundNextDay, @AllowNoAssistGoals, @NumberOfTeamsToParticipate, @SeasonStartDay)";
            return await QuerySingleAsync<SeasonSettings>(createSeasonSettings, createSeasonSettingsCommand);
        }

        public async Task<SeasonSettings> GetSeasonSettingsBySeasonId(int seasonId)
        {
            string getSeasonSettingsBySeasonIdQuery = @"SELECT * FROM SeasonSettings WITH(NOLOCK) WHERE SeasonId=@SeasonId";
            return await GetAsync<SeasonSettings>(getSeasonSettingsBySeasonIdQuery, new { SeasonId = seasonId });
        }

        public async Task<bool> DeleteSeasonSettingsBySeasonId(int seasonId)
        {
            string deleteSeasonSettingsBySeasonIdQuery = @"DELETE FROM SeasonSettings WHERE SeasonId=@SeasonId";
            return await ExecuteAsync(deleteSeasonSettingsBySeasonIdQuery, new { SeasonId = seasonId }) > 0;
        }
    }
}
