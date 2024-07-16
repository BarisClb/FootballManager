using FootballManager.Application.Models.Dtos;

namespace FootballManager.Application.Models.Requests
{
    public class CreateTeamRequest
    {
        public string? TeamName { get; set; }
        public List<BulkInsertPlayer>? Players { get; set; }
        // NOTE - If there was a 'Login' mechanic, We could just send the Id instead of 'UserUsername' and 'UserPassword'
        //public int? ManagerId { get; set; }
        public string? UserUsername { get; set; }
        public string? UserPassword { get; set; }
        public int? SeasonId { get; set; }
        public string? SeasonPassPassword { get; set; }
    }
}
