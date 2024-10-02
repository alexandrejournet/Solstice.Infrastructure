using Radiant.Repository.Core;

namespace Radiant.Repository;

public interface IUnitOfWork : IDisposable
{
    TRepository GetRepository<TRepository, TEntity>() where TRepository : ICoreRepository<TEntity> where TEntity : class;
}
