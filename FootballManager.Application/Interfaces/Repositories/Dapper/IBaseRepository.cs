namespace FootballManager.Application.Interfaces.Repositories.Dapper
{
    public interface IBaseRepository
    {
        Task<TEntity> GetAsync<TEntity>(string sqlQuery, object param);
        Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(string sqlQuery, object param);
        Task<int> ExecuteAsync(string sqlQuery, object param);
        Task<TEntity> QuerySingleAsync<TEntity>(string sqlQuery, object param);
        Task BulkInsertAsync<TEntity>(List<TEntity> objects);
        Task BulkUpdateAsync<TEntity>(List<TEntity> objects);
    }
}
