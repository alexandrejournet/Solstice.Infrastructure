using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Solstice.Domain.Exceptions;

namespace Solstice.Infrastructure.UnitOfWorks;

public sealed class UnitOfWork<TDbContext>(TDbContext dbContext, IHttpContextAccessor httpContext) : IUnitOfWork
    where TDbContext : DbContext
{
    private Dictionary<Type, object> _repositories { get; } = new();

    TRepository IUnitOfWork.GetRepository<TRepository, TEntity>()
    {
        if (_repositories.ContainsKey(typeof(TEntity)))
        {
            return (TRepository)_repositories[typeof(TEntity)];
        }

        var type = typeof(TRepository);
        var repository = Activator.CreateInstance(type, dbContext, httpContext) ?? throw new CoreException("Cannot create repository");
        _repositories.Add(typeof(TEntity), repository);
        return (TRepository)repository;
    }

    public void Dispose()
    {
        dbContext.Dispose();
    }
}
