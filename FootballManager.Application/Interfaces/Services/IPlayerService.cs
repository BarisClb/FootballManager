using FootballManager.Application.Models.DbCommands;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<Player> CreatePlayer(PlayerCommand createPlayerCommand);
    }
}
