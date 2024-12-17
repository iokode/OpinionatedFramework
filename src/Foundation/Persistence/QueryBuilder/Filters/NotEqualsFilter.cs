namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record NotEqualsFilter(string FieldName, object? Value) : Filter;