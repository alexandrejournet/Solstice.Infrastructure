using Solstice.Domain.Models;

namespace Solstice.Infrastructure.Extensions;

/// <summary>
/// Represents a utility class that provides extension methods to IQueryable interface objects.
/// </summary>
/// <remarks>
/// CoreQueryableExtension offers pagination for queries.
/// </remarks>
public static class CoreQueryableExtension
{
    /// <summary>
    /// Returns a paged source from the provided IQueryable object using the given CorePage object parameters.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="queryable">The System.Linq.IQueryable{T} to create a paged source from.</param>
    /// <param name="page">CorePage object that determines the number of elements to bypass and the size of the page.</param>
    /// <returns>An System.Linq.IQueryable{T} that contains elements from the input sequence that occur after the specified index and has the specified page size.</returns>
    public static IQueryable<TSource> Pageable<TSource>(this IQueryable<TSource> queryable, Page page)
    {
        return queryable.Skip(page.LineToSkip()).Take(page.PageSize);
    }
}