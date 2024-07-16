using Dapper;
using FootballManager.Application.Interfaces.Repositories.Dapper;
using FootballManager.Application.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;
using Z.Dapper.Plus;
using static Dapper.SqlMapper;

namespace FootballManager.Persistence.Repositories.Dapper
{
    public class BaseRepository : IBaseRepository
    {
        private readonly IOptions<MsSqlSettings> _msSqlSettings;

        public BaseRepository(IOptions<MsSqlSettings> msSqlSettings)
        {
            _msSqlSettings = msSqlSettings ?? throw new ArgumentNullException(nameof(msSqlSettings));
        }


        public async Task<TEntity> GetAsync<TEntity>(string sqlQuery, object param)
        {
            using (SqlConnection connection = new SqlConnection(_msSqlSettings.Value.ConnectionString))
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                return await connection.QueryFirstOrDefaultAsync<TEntity>(sqlQuery, param);
            }
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(string sqlQuery, object param)
        {
            using (SqlConnection connection = new SqlConnection(_msSqlSettings.Value.ConnectionString))
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                return await connection.QueryAsync<TEntity>(sqlQuery, param);
            }
        }
        public async Task<int> ExecuteAsync(string sqlQuery, object param)
        {
            using (SqlConnection connection = new SqlConnection(_msSqlSettings.Value.ConnectionString))
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                return await connection.ExecuteAsync(sqlQuery, param);
            }
        }
        public async Task<T> QuerySingleAsync<T>(string sqlQuery, object param)
        {
            using (SqlConnection connection = new SqlConnection(_msSqlSettings.Value.ConnectionString))
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                return await connection.QuerySingleAsync<T>(sqlQuery, param);
            }
        }

        public async Task BulkInsertAsync<T>(List<T> objects)
        {
            using (SqlConnection connection = new SqlConnection(_msSqlSettings.Value.ConnectionString))
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                await connection.BulkActionAsync(t => t.BulkInsert(objects));
            }
        }

        public async Task BulkUpdateAsync<T>(List<T> objects)
        {
            using (SqlConnection connection = new SqlConnection(_msSqlSettings.Value.ConnectionString))
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                await connection.BulkActionAsync(t => t.BulkUpdate(objects));
            }
        }

        public async Task BulkDeleteAsync<T>(List<T> objects)
        {
            using (SqlConnection connection = new SqlConnection(_msSqlSettings.Value.ConnectionString))
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                await connection.BulkActionAsync(t => t.BulkDelete(objects));
            }
        }

    }
}
