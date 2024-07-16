using FootballManager.Application.Interfaces.Repositories.EFC;
using FootballManager.Application.Models.Dtos;
using FootballManager.Domain.Entities;
using FootballManager.Persistence.Contexts;

namespace FootballManager.Persistence.Repositories.EFC
{
    public class PlayerEfcRepository : BaseEfcRepository<Player>, IPlayerEfcRepository
    {
        public PlayerEfcRepository(FootballManagerDbContext context) : base(context)
        { }

        public async Task<bool> BulkInsertPlayersForTeam(List<BulkInsertPlayer> playerList)
        {
            List<Player> players = new();

            foreach (var player in playerList)
            {
                Player newPlayer = new();

                newPlayer.Name = player.Name;
                newPlayer.Position = (int)player.Position;
                newPlayer.Number = (int)player.Number;
                newPlayer.TeamId = (int)player.TeamId;
                newPlayer.SeasonId = (int)player.SeasonId;

                players.Add(newPlayer);
            }

            await AddRangeAsync(players);
            return true;
        }
    }
}
