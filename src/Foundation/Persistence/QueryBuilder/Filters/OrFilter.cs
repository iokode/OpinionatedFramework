namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record OrFilter(params Filter[] Filters) : Filter;