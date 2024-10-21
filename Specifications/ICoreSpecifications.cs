using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Solstice.Infrastructure.Specifications;

public interface ICoreSpecifications<T>
{
    // Filter Conditions
    Expression<Func<T, bool>> FilterCondition { get; }
 
    // Order By Ascending
    Collection<Expression<Func<T, object>>> OrderBys { get; }
 
    // Order By Descending
    Collection<Expression<Func<T, object>>> OrderByDescendings { get; }
 
    // Include collection to load related data
    Collection<Expression<Func<T, object>>> SimpleIncludes { get; }
    Collection<Func<IQueryable<T>, IIncludableQueryable<T, object>>> ComplexIncludes { get; }
 
    // GroupBy expression
    Expression<Func<T, object>> GroupBys { get; }
 
    // Distinct
    bool Distincts { get; }
}