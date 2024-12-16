namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record EqualsFilter(string FieldName, object? Value) : Filter;