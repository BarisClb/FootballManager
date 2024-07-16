using FootballManager.Application.Models.DbCommands;
using FootballManager.Application.Models.Responses;
using FootballManager.Domain.Entities;
using FootballManager.Application.Interfaces.Services;

namespace FootballManager.Application.Services;

// The original implementation is withheld from this public historical source.
public sealed class MatchService : IMatchService
{
    public Task<Match> CreateMatch(MatchCommand createMatchCommand) => throw new NotImplementedException("Enter your match logic here.");
    public Task<BaseResponse<Match>> SimulateMatch(int matchId) => throw new NotImplementedException("Enter your match logic here.");
}
