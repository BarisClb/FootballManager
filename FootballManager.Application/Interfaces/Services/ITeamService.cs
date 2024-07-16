using FootballManager.Application.Models.Requests;
using FootballManager.Application.Models.Responses;
using FootballManager.Application.Models.View;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<BaseResponse<Team>> CreateTeam(CreateTeamRequest createTeamRequest);
        Task<BaseResponse<Team>> CreateTeamWithForm(CreateTeamRequestForm createTeamRequestForm);
        Task<BaseResponse<IEnumerable<Team>>> GetTeamsFromTheSeason(int? seasonId);
        Task<SeasonTeamStandings> GetTeamStats(int? teamId);
        Task<CreateTeamRequestForm> GetCreateTeamRequest(int? seasonId, int? premadeTeamId);
    }
}
