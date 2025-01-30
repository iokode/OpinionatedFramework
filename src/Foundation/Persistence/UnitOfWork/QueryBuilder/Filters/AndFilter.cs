namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

public record AndFilter(params Filter[] Filters) : Filter;