namespace IOKode.OpinionatedFramework.Persistence.QueryBuilder.Filters;

public record LikeFilter(string FieldName, string Pattern) : Filter;