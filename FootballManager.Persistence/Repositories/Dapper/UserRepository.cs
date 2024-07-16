using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Models.Requests;
using FootballManager.Application.Settings;
using FootballManager.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FootballManager.Persistence.Repositories.Dapper
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IOptions<MsSqlSettings> msSqlSettings) : base(msSqlSettings)
        { }


        public async Task<User> CreateUser(CreateUserRequest createUserRequest)
        {
            string createUserQuery = @"INSERT INTO Users (Name, Username, Password, Email, Groups, Roles) 
                                       OUTPUT INSERTED.* 
                                       VALUES (@Name, @Username, @Password, @Email, @Groups, @Roles)";
            return await QuerySingleAsync<User>(createUserQuery, createUserRequest);
        }

        public async Task<User> GetUserById(int userId)
        {
            string getUserByIdQuery = @"SELECT * FROM Users WITH(NOLOCK) WHERE Id=@UserId";
            return await GetAsync<User>(getUserByIdQuery, new { UserId = userId });
        }

        public async Task<User> GetFirstUser()
        {
            string getFirstUserQuery = @"SELECT TOP(1) * FROM Users WITH(NOLOCK)";
            return await GetAsync<User>(getFirstUserQuery, null);
        }

        public async Task<User> GetUserByUsernameAndPassword(string username, string password)
        {
            string getUserByNameAndPassword = @"SELECT * FROM Users WITH(NOLOCK) WHERE Username=@Username and Password=@Password";
            return await GetAsync<User>(getUserByNameAndPassword, new { Username = username, Password = password });
        }

        public async Task<IEnumerable<User>> GetManagersByMatchId(int matchId)
        {
            string getUserByNameAndPassword = @"SELECT u.* FROM Matches as m WITH(NOLOCK) 
                                                 LEFT JOIN Users as u WITH(NOLOCK) ON (u.Id = m.HomeTeamManagerId or u.Id = m.AwayTeamManagerId) 
                                                 WHERE m.Id=@MatchId";
            return await GetAllAsync<User>(getUserByNameAndPassword, new { MatchId = matchId });
        }
    }
}
