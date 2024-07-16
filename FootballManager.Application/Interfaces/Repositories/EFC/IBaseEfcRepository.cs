using FootballManager.Domain.Entities;

namespace FootballManager.Application.Interfaces.Repositories.EFC
{
    public interface IBaseEfcRepository<TEntity> where TEntity : BaseEntity
    {
        Task<int> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities);
        Task<int> DeleteRangeAsync(IEnumerable<Guid> entities);
        Task<int> DeleteRangeAsync(IEnumerable<TEntity> entities);
        public int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
