using System;

namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record LessThanFilter(string FieldName, IComparable Value) : Filter;