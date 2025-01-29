namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

public record OrFilter(params Filter[] Filters) : Filter;