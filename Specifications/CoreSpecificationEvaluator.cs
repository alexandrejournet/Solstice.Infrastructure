using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Radiant.Repository.Specifications;

public class CoreSpecificationEvaluator<T> where T : class
{
    protected CoreSpecificationEvaluator()
    {
    }

    public static IQueryable<T> GetQuery(IQueryable<T> query, ICoreSpecifications<T> specifications)
    {
        // Do not apply anything if specifications is null
        if (specifications == null)
        {
            return query;
        }

        // Modify the IQueryable
        // Apply filter conditions
        if (specifications.FilterCondition != null)
        {
            query = query.Where(specifications.FilterCondition);
        }

        // Simple Includes
        if (specifications.SimpleIncludes != null && specifications.SimpleIncludes.Count > 0)
        {
            foreach (var include in specifications.SimpleIncludes)
            {
                query = query.Include(include);
            }
        }

        // Complex Includes (Include with ThenInclude)
        query = specifications.ComplexIncludes
                    .Aggregate(query, (current, include) => include(current));

        // Apply ordering
        if (specifications.OrderBys != null && specifications.OrderBys.Count > 0)
        {
            query = OrderBy(query, specifications.OrderBys);
        }
        else if (specifications.OrderByDescendings != null && specifications.OrderByDescendings.Count > 0)
        {
            query = OrderByDescending(query, specifications.OrderByDescendings);
        }

        // Apply GroupBy
        if (specifications.GroupBys != null)
        {
            query = query.GroupBy(specifications.GroupBys).SelectMany(x => x);
        }

        // Distinct
        if (specifications.Distincts)
        {
            query = query.Distinct();
        }

        return query;
    }

    private static IQueryable<T> OrderBy(IQueryable<T> source, Collection<Expression<Func<T, object>>> expressions)
    {
        IOrderedQueryable<T> orderedResult = null;

        foreach (var expression in expressions)
        {
            if (orderedResult == null)
            {
                orderedResult = source.OrderBy(expression);
            }
            else
            {
                orderedResult = orderedResult.ThenBy(expression);
            }
        }

        return orderedResult ?? source.OrderBy(x => 0); // Return the original order if no expressions are provided
    }

    private static IQueryable<T> OrderByDescending(IQueryable<T> source, Collection<Expression<Func<T, object>>> expressions)
    {
        IOrderedQueryable<T> orderedResult = null;

        foreach (var expression in expressions)
        {
            if (orderedResult == null)
            {
                orderedResult = source.OrderByDescending(expression);
            }
            else
            {
                orderedResult = orderedResult.ThenByDescending(expression);
            }
        }

        return orderedResult ?? source.OrderBy(x => 0); // Return the original order if no expressions are provided
    }
}