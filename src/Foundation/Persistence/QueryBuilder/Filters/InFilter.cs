namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record InFilter(string FieldName, params object[] Values) : Filter;