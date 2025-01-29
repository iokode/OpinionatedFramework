namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

public record InFilter(string FieldName, params object[] Values) : Filter;