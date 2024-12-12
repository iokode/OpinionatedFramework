using System;

namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record GreaterThanFilter(string FieldName, IComparable Value) : Filter;