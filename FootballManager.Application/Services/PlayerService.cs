using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Models.DbCommands;
using FootballManager.Domain.Entities;

namespace FootballManager.Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;

        public PlayerService(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        }


        public async Task<Player> CreatePlayer(PlayerCommand createPlayerCommand)
        {
            return await _playerRepository.CreatePlayer(createPlayerCommand);
        }
    }
}
