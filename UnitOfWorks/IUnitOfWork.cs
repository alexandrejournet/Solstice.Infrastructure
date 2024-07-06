using Radiant.NET.Repository.Core;

namespace Radiant.NET.Repository;

public interface IUnitOfWork : IDisposable
{
    TRepository GetRepository<TRepository, TEntity>() where TRepository : ICoreRepository<TEntity> where TEntity : class;
}
