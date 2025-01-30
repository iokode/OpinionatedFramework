using System;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

public record BetweenFilter(string FieldName, IComparable Low, IComparable High) : Filter;