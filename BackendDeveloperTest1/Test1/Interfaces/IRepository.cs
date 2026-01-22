using Test1.Core;

namespace Test1.Interfaces
{
    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>
    {
        Task<bool> AddAsync(TEntity entity, DapperDbContext dbContext);
        Task<bool> DeleteAsync(Guid gUid, DapperDbContext dbContext);
        Task<bool> UpdateAsync(Guid id, TEntity entity, DapperDbContext dbContext);
    }

    public interface IReadOnlyRepository<TEntity>
    {
        Task<TEntity> GetByIdAsync(Guid id, DapperDbContext dbContext);
        Task<IEnumerable<TEntity>> GetAllAsync(DapperDbContext dbContext);
    }
}
