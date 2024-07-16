using FootballManager.Application.Models.DbCommands;
using FootballManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballManager.Application.Interfaces.Repositories.Dapper
{
    public interface ISeasonSettingsRepository
    {
        Task<SeasonSettings> CreateSeasonSettings(SeasonSettingsCommand createSeasonSettingsCommand);
        Task<SeasonSettings> GetSeasonSettingsBySeasonId(int seasonId);
        Task<bool> DeleteSeasonSettingsBySeasonId(int seasonId);
    }
}
