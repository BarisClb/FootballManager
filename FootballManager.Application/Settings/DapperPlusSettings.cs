using FootballManager.Application.Models.Dtos;
using Z.Dapper.Plus;

namespace FootballManager.Application.Settings
{
    public static class DapperPlusSettings
    {
        public static void RegisterBulkOptions()
        {
            DapperPlusManager.Entity<BulkInsertPlayer>().Table("Players");
            DapperPlusManager.Entity<BulkInsertMatch>().Table("Matches");
            DapperPlusManager.Entity<BulkUpdateSeasonPlacement>().Table("Teams").Key("Id");
        }
    }
}
