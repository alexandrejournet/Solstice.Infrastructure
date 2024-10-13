using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Solstice.Domain.Exceptions;
using Solstice.Domain.Models;
using Solstice.Repository.Extensions;
using System.Data.Common;
using System.Linq.Expressions;
using Solstice.Repository.Specifications;

namespace Solstice.Repository.Core;

/// <summary>
/// An abstract class for the CoreRepository. Constructs a repository with a given context and Http context.
/// </summary>
/// <typeparam name="T">A entity type</typeparam>
/// <typeparam name="TContext">The DbContext type</typeparam>
public class CoreRepository<T, TContext> : ICoreRepository<T>
    where T : class
    where TContext : DbContext
{
    private readonly CancellationToken _cancellationToken;
    private readonly TContext _dbContext;

    /// <summary>
    /// Constructs a new instance of CoreRepository with the given context and Http context.
    /// </summary>
    /// <param name="dbContext">The DbContext to use.</param>
    /// <param name="httpContext">The HttpContext to use for cancellations.</param>
    protected CoreRepository(TContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _cancellationToken = httpContext.HttpContext?.RequestAborted ?? CancellationToken.None;
    }

    /// <summary>
    /// Adds the given entity to the database and saves the changes.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public async Task AddAsync(T entity)
    {
        await _dbContext.AddAsync(entity, _cancellationToken);
    }

    public async Task AddAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, _cancellationToken);
    }

    public async Task AddAndSaveAsync(T entity)
    {
        await AddAsync(entity);
        await SaveAsync();
    }

    public async Task AddAndSaveAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await AddAsync(entity);
        await SaveAsync();
    }

    /// <summary>
    /// Adds a range of entities to the database and saves the changes.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    public async Task AddRangeAsync(ICollection<T> entities)
    {
        await _dbContext.AddRangeAsync(entities, _cancellationToken);
    }

    public async Task AddRangeAsync<TEntity>(ICollection<TEntity> entities) where TEntity : class
    {
        await _dbContext.Set<TEntity>().AddRangeAsync(entities, _cancellationToken);
    }

    public async Task AddRangeAndSaveAsync(ICollection<T> entities)
    {
        await AddRangeAsync(entities);
        await SaveAsync();
    }

    public async Task AddRangeAndSaveAsync<TEntity>(ICollection<TEntity> entities) where TEntity : class
    {
        await AddRangeAsync(entities);
        await SaveAsync();
    }

    /// <summary>
    /// Checks if any entities in the database match the given expression.
    /// </summary>
    /// <param name="where">The expression to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean
    /// indicating whether any entities match the expression.</returns>
    public async Task<bool> AnyAsyncBy(Expression<Func<T, bool>> where)
    {
        return await CoreSpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>()
                .AsQueryable(), new Specification<T>(where))
            .AsNoTracking()
            .AnyAsync(_cancellationToken);
    }

    public async Task<bool> AnyAsyncBy<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
    {
        return await ApplySpecificationWhere(where).AnyAsync(_cancellationToken);
    }
    public async Task<bool> AnyAsyncBy<TEntity>(IQueryable<TEntity> queryable) where TEntity : class
    {
        return await queryable.AnyAsync(_cancellationToken);
    }
    /// <summary>
    /// Begins a new transaction in the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the IDbContextTransaction
    /// that encapsulates all changes made to the DbContext within the transaction.</returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _dbContext.Database.BeginTransactionAsync(_cancellationToken);
    }

    /// <summary>
    /// Counts all entities in the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the count of all entities.</returns>
    public async Task<decimal> CountAllAsync()
    {
        return await CountAllAsyncBy(null);
    }
    
    /// <summary>
    /// Counts all entities in the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the count of all entities.</returns>
    public async Task<decimal> CountAllAsync<TEntity>() where TEntity : class
    {
        return await CountAllAsyncBy<TEntity>(null);
    }

    /// <summary>
    /// Counts all entities in the database that satisfy the given expression.
    /// </summary>
    /// <param name="where">The expression to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the count of entities that satisfy the expression.</returns>
    public async Task<decimal> CountAllAsyncBy(Expression<Func<T, bool>>? where)
    {
        return await CoreSpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>()
                .AsQueryable(), new Specification<T>(where))
            .CountAsync(_cancellationToken);
    }

    public async Task<decimal> CountAllAsyncBy<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
    {
        return await ApplySpecificationWhere(where).CountAsync(_cancellationToken);
    }

    public async Task<T> FindAsync(int id)
    {
        return await _dbContext.Set<T>().FindAsync(id) ?? throw CoreException.Format(CoreExceptionEnum.HTTP_404);
    }

    public async Task<TEntity> FindAsync<TEntity>(int id) where TEntity : class
    {
        return await _dbContext.Set<TEntity>().FindAsync(id) ?? throw CoreException.Format(CoreExceptionEnum.HTTP_404);
    }

    /// <summary>
    /// Gets all entities from the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of all entities.</returns>
    public async Task<ICollection<T>> GetAllAsync()
    {
        return await GetAllQueryable().ToCollectionAsync(_cancellationToken);
    }
    /// <summary>
    /// Gets all entities from the database that satisfy the given expression.
    /// </summary>
    /// <param name="where">The expression to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities that satisfy the expression.</returns>
    public async Task<ICollection<T>> GetAllAsync(Expression<Func<T, bool>> where)
    {
        return await GetAllQueryable(where).ToCollectionAsync(_cancellationToken);
    }
    /// <summary>
    /// Gets all entities from the database that satisfy The core specifications.
    /// </summary>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities that satisfy the specifications.</returns>
    public async Task<ICollection<T>> GetAllAsync(ICoreSpecifications<T>? coreSpecifications)
    {
        return await GetAllQueryable(coreSpecifications).ToCollectionAsync(_cancellationToken);
    }
    /// <summary>
    /// Gets all entities from the database based on the provided query and specifications.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities fetched based on the query and specifications.</returns>
    public async Task<ICollection<T>> GetAllAsync(string query, ICoreSpecifications<T>? coreSpecifications)
    {
        return await GetAllQueryable(query, coreSpecifications).ToCollectionAsync(_cancellationToken);
    }
    /// <summary>
    /// Gets all entities from the database based on the provided query, parameters and specifications.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">The SQL parameters needed for the query.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities fetched based on the query, parameters, and specifications.</returns>
    public async Task<ICollection<T>> GetAllAsync(string query, ICollection<DbParameter> parameters,
        ICoreSpecifications<T>? coreSpecifications)
    {
        return await GetAllQueryable(query, parameters, coreSpecifications).ToCollectionAsync(_cancellationToken);
    }
    public async Task<ICollection<TEntity>> GetAllAsync<TEntity>() where TEntity : class
    {
        return await ApplySpecification<TEntity>(null).ToCollectionAsync(_cancellationToken);
    }
    public async Task<ICollection<TEntity>> GetAllAsync<TEntity>(ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return await ApplySpecification(coreSpecifications).ToCollectionAsync(_cancellationToken);
    }
    public async Task<ICollection<TEntity>> GetAllAsync<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
    {
        return await ApplySpecificationWhere(where).ToCollectionAsync(_cancellationToken);
    }
    public async Task<ICollection<TEntity>> GetAllAsync<TEntity>(string query) where TEntity : class
    {
        return await ApplySpecificationQuery<TEntity>(query).ToCollectionAsync(_cancellationToken);
    }
    public async Task<ICollection<TEntity>> GetAllAsync<TEntity>(string query, ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return await ApplySpecificationQuery(query, coreSpecifications).ToCollectionAsync(_cancellationToken);
    }
    public async Task<ICollection<TEntity>> GetAllAsync<TEntity>(string query, ICollection<DbParameter> parameters) where TEntity : class
    {
        return await ApplySpecificationQuery<TEntity>(query, parameters).ToCollectionAsync(_cancellationToken);
    }
    public async Task<ICollection<TEntity>> GetAllAsync<TEntity>(string query, ICollection<DbParameter> parameters, ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return await ApplySpecificationQuery(query, parameters, coreSpecifications).ToCollectionAsync(_cancellationToken);
    }
    /// <summary>
    /// Gets all entities from the database by executing the provided IQueryable query.
    /// </summary>
    /// <param name="query">The IQueryable query to execute.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities fetched by executing the query.</returns>
    public async Task<ICollection<TEntity>> GetAllByQueryable<TEntity>(IQueryable<TEntity> query)
    {
        return await query.ToCollectionAsync(_cancellationToken);
    }
    /// <summary>
    /// Gets all entities from the database satisfying the specifications provided.
    /// </summary>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities satisfying the specifications.</returns>
    public IQueryable<T> GetAllQueryable(ICoreSpecifications<T>? coreSpecifications)
    {
        return ApplySpecification(coreSpecifications)
            .AsNoTracking();
    }

    /// <summary>
    /// Gets entities from the database based on the SQL query and specifications provided.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities fetched based on the query and specifications.</returns>
    public IQueryable<T> GetAllQueryable(string query)
    {
        return ApplySpecificationQuery(query)
            .AsNoTracking();
    }

    /// <summary>
    /// Gets entities from the database based on the SQL query and specifications provided.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities fetched based on the query and specifications.</returns>
    public IQueryable<T> GetAllQueryable(string query, ICoreSpecifications<T>? coreSpecifications)
    {
        return ApplySpecificationQuery(query, coreSpecifications)
            .AsNoTracking();
    }

    /// <summary>
    /// Gets entities from the database based on the SQL query, parameters, and specifications provided.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">The SQL parameters needed for the query.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities fetched based on the query, parameters, and specifications.</returns>
    public IQueryable<T> GetAllQueryable(string query, ICollection<DbParameter> parameters)
    {
        return ApplySpecificationQuery(query, parameters)
            .AsNoTracking();
    }

    /// <summary>
    /// Gets entities from the database based on the SQL query, parameters, and specifications provided.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">The SQL parameters needed for the query.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities fetched based on the query, parameters, and specifications.</returns>
    public IQueryable<T> GetAllQueryable(string query, ICollection<DbParameter> parameters,
        ICoreSpecifications<T>? coreSpecifications)
    {
        return ApplySpecificationQuery(query, parameters, coreSpecifications)
            .AsNoTracking();
    }
    /// <summary>
    /// Get all entities as IQueryable.
    /// </summary>
    /// <returns>IQueryable of all entities in the database.</returns>
    public IQueryable<T> GetAllQueryable()
    {
        return CoreSpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>()
                .AsQueryable(), new Specification<T>())
            .AsNoTracking();
    }
    /// <summary>
    /// Gets all entities from the database that matches the given expression as IQueryable.
    /// </summary>
    /// <param name="where">The expression to evaluate.</param>
    /// <returns>IQueryable of entities that match the expression.</returns>
    public IQueryable<T> GetAllQueryable(Expression<Func<T, bool>> where)
    {
        return ApplySpecificationWhere(where)
            .AsNoTracking();
    }

    /// <summary>
    /// Gets all entities from the database satisfying the specifications provided.
    /// </summary>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities satisfying the specifications.</returns>
    public IQueryable<TEntity> GetAllQueryable<TEntity>(ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return ApplySpecification(coreSpecifications)
            .AsNoTracking();
    }

    /// <summary>
    /// Gets entities from the database based on the SQL query and specifications provided.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities fetched based on the query and specifications.</returns>
    public IQueryable<TEntity> GetAllQueryable<TEntity>(string query) where TEntity : class
    {
        return ApplySpecificationQuery<TEntity>(query).AsNoTracking();
    }

    /// <summary>
    /// Gets entities from the database based on the SQL query and specifications provided.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities fetched based on the query and specifications.</returns>
    public IQueryable<TEntity> GetAllQueryable<TEntity>(string query, ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return ApplySpecificationQuery(query, coreSpecifications)
            .AsNoTracking();
    }

    /// <summary>
    /// Gets entities from the database based on the SQL query, parameters, and specifications provided.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">The SQL parameters needed for the query.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities fetched based on the query, parameters, and specifications.</returns>
    public IQueryable<TEntity> GetAllQueryable<TEntity>(string query, ICollection<DbParameter> parameters) where TEntity : class
    {
        return ApplySpecificationQuery<TEntity>(query, parameters)
            .AsNoTracking();
    }

    /// <summary>
    /// Gets entities from the database based on the SQL query, parameters, and specifications provided.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">The SQL parameters needed for the query.</param>
    /// <param name="coreSpecifications">The core specifications to evaluate.</param>
    /// <returns>IQueryable of entities fetched based on the query, parameters, and specifications.</returns>
    public IQueryable<TEntity> GetAllQueryable<TEntity>(string query, ICollection<DbParameter> parameters,
        ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return ApplySpecificationQuery(query, parameters, coreSpecifications)
            .AsNoTracking();
    }
    /// <summary>
    /// Get all entities as IQueryable.
    /// </summary>
    /// <returns>IQueryable of all entities in the database.</returns>
    public IQueryable<TEntity> GetAllQueryable<TEntity>() where TEntity : class
    {
        return CoreSpecificationEvaluator<TEntity>.GetQuery(_dbContext.Set<TEntity>()
                .AsQueryable(), new Specification<TEntity>())
            .AsNoTracking();
    }
    /// <summary>
    /// Gets all entities from the database that matches the given expression as IQueryable.
    /// </summary>
    /// <param name="where">The expression to evaluate.</param>
    /// <returns>IQueryable of entities that match the expression.</returns>
    public IQueryable<TEntity> GetAllQueryable<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
    {
        return ApplySpecificationWhere(where)
            .AsNoTracking();
    }

    /// <summary>
    /// Gets the first entity that satisfies the provided expression.
    /// </summary>
    /// <param name="where">The expression to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that satisfies the expression or null if no such entity exists.</returns>
    public async Task<T?> GetBy(Expression<Func<T, bool>> where)
    {
        return await ApplySpecificationWhere(where)
            .AsNoTracking()
            .FirstOrDefaultAsync(_cancellationToken);
    }
    /// <summary>
    /// Gets the first entity that satisfies the provided specifications.
    /// </summary>
    /// <param name="coreSpecifications">The specifications to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that satisfies the specifications or null if no such entity exists.</returns>
    public async Task<T?> GetBy(ICoreSpecifications<T>? coreSpecifications)
    {
        return await ApplySpecification(coreSpecifications)
            .AsNoTracking()
            .FirstOrDefaultAsync(_cancellationToken);
    }
    public async Task<TEntity?> GetBy<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
    {
        return await ApplySpecificationWhere(where).FirstOrDefaultAsync(_cancellationToken);
    }
    public async Task<TEntity?> GetBy<TEntity>(ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return await ApplySpecification(coreSpecifications).FirstOrDefaultAsync(_cancellationToken);
    }
    public async Task<TEntity?> GetBy<TEntity>(string query, ICollection<DbParameter> parameters, ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return await ApplySpecificationQuery(query, parameters, coreSpecifications).FirstOrDefaultAsync(_cancellationToken);
    }
    public async Task<Paged<T>> GetPagedResult(Page page)
    {
        return await GetPagedResult(page, null);
    }
    public async Task<Paged<T>> GetPagedResult(Page page, ICoreSpecifications<T>? coreSpecifications)
    {
        return new()
        {
            Items = await ApplySpecification(coreSpecifications)
                .Pageable(page)
                .AsNoTracking()
                .ToCollectionAsync(_cancellationToken),
            Count = await ApplySpecification(coreSpecifications).CountAsync(_cancellationToken)
        };
    }
    public async Task<Paged<T>> GetPagedResult(Page page, string query, ICollection<DbParameter> parameters)
    {
        return await GetPagedResult(page, query, parameters, null);
    }
    public async Task<Paged<T>> GetPagedResult(Page page, string query, ICollection<DbParameter> parameters, ICoreSpecifications<T>? coreSpecifications)
    {
        return new()
        {
            Items = await ApplySpecificationQuery(query, parameters, coreSpecifications)
                .Pageable(page)
                .AsNoTracking()
                .ToCollectionAsync(_cancellationToken),
            Count = await ApplySpecificationQuery(query, parameters, coreSpecifications)
                .CountAsync(_cancellationToken)
        };
    }
    public async Task<Paged<TEntity>> GetPagedResult<TEntity>(Page page) where TEntity : class
    {
        return await GetPagedResult<TEntity>(page, null);
    }
    public async Task<Paged<TEntity>> GetPagedResult<TEntity>(Page page, ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return new()
        {
            Items = await ApplySpecification(coreSpecifications)
                .Pageable(page)
                .AsNoTracking()
                .ToCollectionAsync(_cancellationToken),
            Count = await ApplySpecification(coreSpecifications)
                .CountAsync(_cancellationToken)
        };
    }
    public async Task<Paged<TEntity>> GetPagedResult<TEntity>(Page page, string query, ICollection<DbParameter> parameters) where TEntity : class
    {
        return await GetPagedResult<TEntity>(page, query, parameters, null);
    }
    public async Task<Paged<TEntity>> GetPagedResult<TEntity>(Page page, string query, ICollection<DbParameter> parameters, ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return new()
        {
            Items = await ApplySpecificationQuery(query, parameters, coreSpecifications)
                .Pageable(page)
                .AsNoTracking()
                .ToCollectionAsync(_cancellationToken),
            Count = await ApplySpecificationQuery(query, parameters, coreSpecifications)
                .CountAsync(_cancellationToken)
        };
    }
    /// <summary>
    /// Pages all entities based on the provided page information and specifications.
    /// </summary>
    /// <param name="page">The page information.</param>
    /// <param name="coreSpecifications">The specifications to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities paged according to the provided information and specifications.</returns>
    public async Task<ICollection<T>> PageAllAsync(Page page,
        ICoreSpecifications<T>? coreSpecifications)
    {
        return await PageAllQueryable(page, coreSpecifications).ToCollectionAsync(_cancellationToken);
    }
    /// <summary>
    /// Returns an IQueryable of all paged entities based on the provided page information and specifications.
    /// </summary>
    /// <param name="page">The page information.</param>
    /// <param name="coreSpecifications">The specifications to evaluate.</param>
    /// <returns>IQueryable of entities paged according to the provided page information and specifications.</returns>
    public IQueryable<T> PageAllQueryable(Page page, ICoreSpecifications<T>? coreSpecifications)
    {
        page ??= new Page();

        return ApplySpecification(coreSpecifications)
            .Pageable(page)
            .AsNoTracking();
    }
    /// <summary>
    /// Removes the given entity from the database and saves the changes.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public void Remove(T entity)
    {
        _dbContext.Remove(entity);
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }
    public async Task RemoveAndSaveAsync(T entity)
    {
        Remove(entity);
        await SaveAsync();
    }
    public async Task RemoveAndSaveAsync<TEntity>(TEntity entity) where TEntity : class
    {
        Remove(entity);
        await SaveAsync();
    }
    /// <summary>
    /// Removes a range of entities from the database and saves the changes.
    /// </summary>
    /// <param name="entities">The entities to removal.</param>
    public void RemoveRange(ICollection<T> entities)
    {
        _dbContext.RemoveRange(entities);
    }
    public void RemoveRange<TEntity>(ICollection<TEntity> entities) where TEntity : class
    {
        _dbContext.Set<TEntity>().RemoveRange(entities);
    }
    public async Task RemoveRangeAndSaveAsync(ICollection<T> entities)
    {
        RemoveRange(entities);
        await SaveAsync();
    }
    public async Task RemoveRangeAndSaveAsync<TEntity>(ICollection<TEntity> entities) where TEntity : class
    {
        RemoveRange(entities);
        await SaveAsync();
    }
    /// <summary>
    /// Saves changes in the DbContext to the database.
    /// </summary>
    /// <returns>A task represents the asynchronous operation for saving changes to the database.</returns>
    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync(_cancellationToken);
    }

    /// <summary>
    /// Updates the provided entity in the DbContext and saves the changes to the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task representing the asynchronous operation of updating the entity and saving changes to the database.</returns>
    public void Update(T entity)
    {
        _dbContext.Update(entity);
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        _dbContext.Set<TEntity>().Update(entity);
    }
    public async Task UpdateAndSaveAsync(T entity)
    {
        Update(entity);
        await SaveAsync();
    }
    public async Task UpdateAndSaveAsync<TEntity>(TEntity entity) where TEntity : class
    {
        Update(entity);
        await SaveAsync();
    }
    /// <summary>
    /// Updates the range of entities in the DbContext and saves the changes to the database.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <returns>A task representing async operation of updating the entities and saving changes to the database.</returns>
    public void UpdateRange(ICollection<T> entities)
    {
        _dbContext.UpdateRange(entities);
    }

    public void UpdateRange<TEntity>(ICollection<TEntity> entities) where TEntity : class
    {
        _dbContext.Set<TEntity>().UpdateRange(entities);
    }
    public async Task UpdateRangeAndSaveAsync(ICollection<T> entities)
    {
        UpdateRange(entities);
        await SaveAsync();
    }
    public async Task UpdateRangeAndSaveAsync<TEntity>(ICollection<TEntity> entities) where TEntity : class
    {
        UpdateRange(entities);
        await SaveAsync();
    }

    public async Task ExecuteQuery(string query, ICollection<DbParameter>? dbParameters)
    {
        if (dbParameters is not null)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(query, dbParameters, _cancellationToken);
        }
        else
        {
            await _dbContext.Database.ExecuteSqlRawAsync(query, _cancellationToken);
        }

        await SaveAsync();
    }

    private IQueryable<T> ApplySpecification(ICoreSpecifications<T>? coreSpecifications)
    {
        return CoreSpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>().AsQueryable(), coreSpecifications);
    }

    private IQueryable<TEntity> ApplySpecification<TEntity>(ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return CoreSpecificationEvaluator<TEntity>.GetQuery(_dbContext.Set<TEntity>().AsQueryable(), coreSpecifications);
    }
    private IQueryable<T> ApplySpecificationQuery(string query)
    {
        return CoreSpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>()
            .FromSqlRaw(query)
            .AsQueryable(), null);
    }

    private IQueryable<T> ApplySpecificationQuery(string query, ICoreSpecifications<T>? coreSpecifications)
    {
        return CoreSpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>()
            .FromSqlRaw(query)
            .AsQueryable(), coreSpecifications);
    }

    private IQueryable<T> ApplySpecificationQuery(string query, ICollection<DbParameter> parameters)
    {
        return CoreSpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>()
            .FromSqlRaw(query, parameters.ToArray())
            .AsQueryable(), null);
    }

    private IQueryable<T> ApplySpecificationQuery(string query, ICollection<DbParameter> parameters, ICoreSpecifications<T>? coreSpecifications)
    {
        return CoreSpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>()
            .FromSqlRaw(query, parameters.ToArray())
            .AsQueryable(), coreSpecifications);
    }

    private IQueryable<TEntity> ApplySpecificationQuery<TEntity>(string query) where TEntity : class
    {
        return CoreSpecificationEvaluator<TEntity>.GetQuery(_dbContext.Set<TEntity>()
            .FromSqlRaw(query)
            .AsQueryable(), null);
    }
    private IQueryable<TEntity> ApplySpecificationQuery<TEntity>(string query, ICollection<DbParameter> parameters) where TEntity : class
    {
        return CoreSpecificationEvaluator<TEntity>.GetQuery(_dbContext.Set<TEntity>()
            .FromSqlRaw(query, parameters.ToArray())
            .AsQueryable(), null);
    }
    private IQueryable<TEntity> ApplySpecificationQuery<TEntity>(string query, ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return CoreSpecificationEvaluator<TEntity>.GetQuery(_dbContext.Set<TEntity>()
            .FromSqlRaw(query)
            .AsQueryable(), coreSpecifications);
    }
    private IQueryable<TEntity> ApplySpecificationQuery<TEntity>(string query, ICollection<DbParameter> parameters, ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return CoreSpecificationEvaluator<TEntity>.GetQuery(_dbContext.Set<TEntity>()
            .FromSqlRaw(query, parameters.ToArray())
            .AsQueryable(), coreSpecifications);
    }
    private IQueryable<T> ApplySpecificationWhere(Expression<Func<T, bool>> where)
    {
        return ApplySpecificationWhere(where, null);
    }
    private IQueryable<T> ApplySpecificationWhere(Expression<Func<T, bool>> where, ICoreSpecifications<T>? coreSpecifications)
    {
        return CoreSpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>().Where(where).AsQueryable(), coreSpecifications);
    }
    private IQueryable<TEntity> ApplySpecificationWhere<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
    {
        return ApplySpecificationWhere(where, null);
    }
    private IQueryable<TEntity> ApplySpecificationWhere<TEntity>(Expression<Func<TEntity, bool>> where, ICoreSpecifications<TEntity>? coreSpecifications) where TEntity : class
    {
        return CoreSpecificationEvaluator<TEntity>.GetQuery(_dbContext.Set<TEntity>().Where(where).AsQueryable(), coreSpecifications);
    }
}

public static class CoreRepositoryExtension
{
    /// <summary>
    /// Converts provided IQueryable of entities into a list of entities in an asynchronous manner,
    /// respecting the provided cancellation token.
    /// </summary>
    /// <typeparam name="T">The type of entities</typeparam>
    /// <param name="query">The IQueryable of entities to be converted into a list</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task that represents the asynchronous operation,
    /// with a return value of the list containing the entities</returns>
    public static async Task<ICollection<T>> ToCollectionAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken)
    {
        return await query.ToListAsync(cancellationToken);
    }
}