using FootballManager.Domain.Enums;

namespace FootballManager.Application.Models.Requests
{
    public class CreateSeasonRequest
    {
        public string Name { get; set; }
        public SeasonType? SeasonType { get; set; }
    }
}
