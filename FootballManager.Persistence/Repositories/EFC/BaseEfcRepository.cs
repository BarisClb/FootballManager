using FootballManager.Application.Interfaces.Repositories.EFC;
using FootballManager.Domain.Entities;
using FootballManager.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Persistence.Repositories.EFC
{
    public class BaseEfcRepository<TEntity> : IBaseEfcRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly FootballManagerDbContext _context;
        protected readonly DbSet<TEntity> _entity;

        public BaseEfcRepository(FootballManagerDbContext context)
        {
            _context = context;
            _entity = _context.Set<TEntity>();
        }


        public async Task<int> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _entity.AddRangeAsync(entities);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            _entity.UpdateRange(entities);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteRangeAsync(IEnumerable<Guid> entities)
        {
            foreach (var entityId in entities)
            {
                var entity = await _entity.FindAsync(entityId);
                if (entity != null)
                    _entity.Remove(entity);
            }
            var deleted = await _context.SaveChangesAsync();
            return deleted;
        }

        public async Task<int> DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _entity.Remove(entity);
            }
            var deleted = await _context.SaveChangesAsync();
            return deleted;
        }

        // Helpers

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
