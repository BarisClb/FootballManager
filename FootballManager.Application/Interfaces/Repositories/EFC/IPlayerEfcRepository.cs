using FootballManager.Application.Models.Dtos;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.EFC
{
    public interface IPlayerEfcRepository : IBaseEfcRepository<Player>
    {
        Task<bool> BulkInsertPlayersForTeam(List<BulkInsertPlayer> playerList);
    }
}
