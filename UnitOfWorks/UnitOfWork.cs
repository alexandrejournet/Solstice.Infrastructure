using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Solstice.Domain.Exceptions;

namespace Solstice.Infrastructure.UnitOfWorks;

public sealed class UnitOfWork<TDbContext>(TDbContext dbContext, IHttpContextAccessor httpContext) : IUnitOfWork
    where TDbContext : DbContext
{
    private Dictionary<Type, object> Repositories { get; } = new();

    TRepository IUnitOfWork.GetRepository<TRepository, TEntity>()
    {
        if (Repositories.ContainsKey(typeof(TEntity)))
        {
            return (TRepository)Repositories[typeof(TEntity)];
        }

        var type = typeof(TRepository);
        var repository = Activator.CreateInstance(type, dbContext, httpContext) ?? throw new CoreException("Cannot create repository");
        Repositories.Add(typeof(TEntity), repository);
        return (TRepository)repository;
    }

    public void Dispose()
    {
        dbContext.Dispose();
    }
}
