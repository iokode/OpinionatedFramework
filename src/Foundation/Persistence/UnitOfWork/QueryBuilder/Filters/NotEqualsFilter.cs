namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

public record NotEqualsFilter(string FieldName, object? Value) : Filter;