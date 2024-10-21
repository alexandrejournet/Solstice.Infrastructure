using Solstice.Infrastructure.Core;

namespace Solstice.Infrastructure.UnitOfWorks;

public interface IUnitOfWork : IDisposable
{
    TRepository GetRepository<TRepository, TEntity>() where TRepository : ICoreRepository<TEntity> where TEntity : class;
}
