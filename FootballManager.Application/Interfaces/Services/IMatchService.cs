using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Responses;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Services
{
    public interface IMatchService
    {
        Task<Match> CreateMatch(MatchCommand createMatchCommand);
        Task<BaseResponse<Match>> SimulateMatch(int matchId);
    }
}
