namespace IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Filters;

public record LikeFilter(string FieldName, string Pattern) : Filter;