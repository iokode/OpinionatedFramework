using System;

namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record BetweenFilter(string FieldName, IComparable Low, IComparable High) : Filter;