using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Solstice.Infrastructure.Specifications;

/// <summary>
/// The `coreSpecifications&lt;T&gt;` class implements the `IcoreSpecifications&lt;T&gt;` interface for providing generic ways to
/// define specifications for querying the database.
/// </summary>
/// <typeparam name="T">The type of the object which specifications are applied to. Something like an EF Core model type.</typeparam>
/// <remarks>
/// Properties and methods in this class allow for including related data, ordering, filtering, and grouping the result of queries
/// against a DBSet of the specified type <typeparamref name="T"/>.
/// Note: The 'Include', 'OrderBy', 'OrderByDescending', 'FilterCondition', and 'GroupBy' are Expressions and something like LINQ queries.
/// </remarks>
/// <example>
/// This sample shows how to create a new instance of `coreSpecifications&lt;T&gt;`.
/// <code>
/// var specs = new coreSpecifications&lt;MyModel&gt;();
/// specs.SetFilterCondition(x => x.Property > 0);
/// specs.ApplyOrderBy(x => x.AnotherProperty);
/// </code>
/// </example>
public class Specification<T> : ICoreSpecifications<T>
{
    public Specification()
    { }

    public Specification(Expression<Func<T, bool>>? filterCondition)
    {
        if (filterCondition != null)
        {
            FilterCondition = filterCondition;
        }
    }

    public Expression<Func<T, bool>> FilterCondition { get; private set; }
    public Collection<Expression<Func<T, object>>> SimpleIncludes { get; } = new();
    public Collection<Func<IQueryable<T>, IIncludableQueryable<T, object>>> ComplexIncludes { get; } = new();
    public Collection<Expression<Func<T, object>>> OrderBys { get; } = new();
    public Collection<Expression<Func<T, object>>> OrderByDescendings { get; } = new();
    public Expression<Func<T, object>> GroupBys { get; private set; }
    public bool Distincts { get; private set; }

    public void SetFilterCondition(Expression<Func<T, bool>> filterExpression)
    {
        FilterCondition = filterExpression;
    }

    /// <summary>
    /// Permet d'effectuer un .Include sur une entité
    /// </summary>
    /// <remarks>Exemple: new Specification().Include(item => item.User)</remarks>
    /// <param name="includeExpression"></param>
    /// <returns></returns>
    public Specification<T> Include(Expression<Func<T, object>> includeExpression)
    {
        SimpleIncludes.Add(includeExpression);
        return this;
    }

    /// <summary>
    /// Permet d'effectuer un .Include sur une entité
    /// </summary>
    /// <remarks>Exemple: new Specification().Include(item => item.Include(item => item.User).ThenInclude(item => item.Address))</remarks>
    /// <param name="includeExpression"></param>
    /// <returns></returns>
    public Specification<T> Include(Func<IQueryable<T>, IIncludableQueryable<T, object>> includeExpression)
    {
        ComplexIncludes.Add(includeExpression);
        return this;
    }

    /// <summary>
    /// Permet d'ajouter un OrderBy à la requête<br/>
    /// Si on en ajoute plusieurs, ils seront exécutés dans l'ordre d'ajout (OrderBy, ThenBy, ThenBy, ...)
    /// </summary>
    /// <param name="orderByExpression"></param>
    /// <returns></returns>
    public Specification<T> OrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBys.Add(orderByExpression);
        return this;
    }

    /// <summary>
    /// Permet d'ajouter un OrderByDescending à la requête<br/>
    /// Si on en ajoute plusieurs, ils seront exécutés dans l'ordre d'ajout (OrderByDescending, ThenByDescending, ThenByDescending, ...)
    /// </summary>
    /// <param name="orderByDescendingExpression"></param>
    /// <returns></returns>
    public Specification<T> OrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescendings.Add(orderByDescendingExpression);
        return this;
    }

    /// <summary>
    /// Permet d'ajouter un GroupBy à la requête
    /// </summary>
    /// <param name="groupByExpression"></param>
    public Specification<T> GroupBy(Expression<Func<T, object>> groupByExpression)
    {
        GroupBys = groupByExpression;
        return this;
    }

    /// <summary>
    /// Permet d'ajouter un Distinct à la requête
    /// </summary>
    public Specification<T> Distinct()
    {
        Distincts = true;
        return this;
    }
}