using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record InFilter(string FieldName, IEnumerable<object> Values) : Filter;