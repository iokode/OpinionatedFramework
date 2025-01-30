using System;

namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

public record LessThanFilter(string FieldName, IComparable Value) : Filter;