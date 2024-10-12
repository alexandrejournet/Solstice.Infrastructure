using Solstice.Repository.Core;

namespace Solstice.Repository;

public interface IUnitOfWork : IDisposable
{
    TRepository GetRepository<TRepository, TEntity>() where TRepository : ICoreRepository<TEntity> where TEntity : class;
}
