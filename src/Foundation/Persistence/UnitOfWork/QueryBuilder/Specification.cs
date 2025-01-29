using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder;

public abstract class Specification
{
    private readonly List<Filter> filters = new();

    protected void AddFilter(Filter filter) => this.filters.Add(filter);

    /// <summary>
    /// Converts this specification into a filter.
    /// </summary>
    /// <returns>The filter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no filters are defined in this specification.</exception>
    public Filter ToFilter()
    {
        // If there is only one filter, return it directly; 
        // if multiple, combine them with AND logic by default.
        return this.filters.Count switch
        {
            0 => throw new InvalidOperationException("No filters defined in this specification."),
            1 => this.filters[0],
            _ => new AndFilter(this.filters.ToArray())
        };
    }

    public static implicit operator Filter(Specification spec)
    {
        return spec.ToFilter();
    }
}