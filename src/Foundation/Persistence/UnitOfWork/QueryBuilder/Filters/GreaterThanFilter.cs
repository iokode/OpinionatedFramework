using System;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

public record GreaterThanFilter(string FieldName, IComparable Value) : Filter;