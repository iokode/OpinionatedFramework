namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

public record EqualsFilter(string FieldName, object? Value) : Filter;