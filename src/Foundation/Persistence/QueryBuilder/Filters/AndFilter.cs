namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record AndFilter(params Filter[] Filters) : Filter;