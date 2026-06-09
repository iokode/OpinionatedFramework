using System;
using System.Linq;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Utilities;

namespace IOKode.OpinionatedFramework.Tests.Foundation.Foundation.Utilities;

public class SqlUtilitiesTests
{
    [Fact]
    public void GetDirectives_ReturnsAllSqlDirectives()
    {
        var sql = """
                  -- @generate
                  -- @parameter string name
                  select * from users
                    -- @OnlyActive
                  """;

        var directives = SqlUtilities.GetDirectives(sql);

        Assert.Equal(new[] { "generate", "parameter string name", "OnlyActive" }, directives);
    }

    [Fact]
    public void GetQueryBlocks_WithoutResultSet_ReturnsAllDirectivesAsGlobalAndImplicitResultSetDirectives()
    {
        var sql = """
                  -- @generate
                  -- @tenant_scoped
                  -- @cardinality one
                  -- @result int id
                  select id from users
                  """;

        var queryBlocks = SqlUtilities.GetQueryBlocks(sql);

        Assert.False(queryBlocks.HasExplicitResultSets);
        Assert.Equal(new[] { "generate", "tenant_scoped", "cardinality one", "result int id" }, queryBlocks.GlobalDirectives);
        Assert.Single(queryBlocks.ResultSets);
        Assert.Equal(queryBlocks.GlobalDirectives, queryBlocks.ResultSets.Single().Directives);
    }

    [Fact]
    public void GetQueryBlocks_WithResultSets_SplitsDirectivesByPosition()
    {
        var sql = """
                  -- @generate
                  -- @tenant_scoped

                  -- @result_set users
                  -- @only_active
                  -- @cardinality zero_or_more
                  -- @result int id
                  select id from users;

                  -- @result_set total
                  -- @cache 30s
                  -- @cardinality one
                  -- @scalar_result int total
                  select count(*) as total from users;
                  """;

        var queryBlocks = SqlUtilities.GetQueryBlocks(sql);

        Assert.True(queryBlocks.HasExplicitResultSets);
        Assert.Equal(new[] { "generate", "tenant_scoped" }, queryBlocks.GlobalDirectives);
        Assert.Equal(2, queryBlocks.ResultSets.Count);
        Assert.Equal(new[] { "result_set users", "only_active", "cardinality zero_or_more", "result int id" }, queryBlocks.ResultSets[0].Directives);
        Assert.Equal(new[] { "result_set total", "cache 30s", "cardinality one", "scalar_result int total" }, queryBlocks.ResultSets[1].Directives);
    }

    [Theory]
    [InlineData(QueryCardinality.ZeroOrMore, null)]
    [InlineData(QueryCardinality.ZeroOrMore, "cardinality zero_or_more")]
    [InlineData(QueryCardinality.One, "cardinality one")]
    [InlineData(QueryCardinality.ZeroOrOne, "cardinality zero_or_one")]
    public void GetCardinality_ReturnsCardinalityFromDirectives(QueryCardinality expectedCardinality,
        string? directive)
    {
        var directives = directive == null ? Array.Empty<string>() : new[] { directive };

        var cardinality = SqlUtilities.GetCardinality(directives);

        Assert.Equal(expectedCardinality, cardinality);
    }

    [Fact]
    public void GetCardinality_ThrowsForIncompatibleCardinalityDirectives()
    {
        Assert.Throws<QueryDefinitionException>(() => SqlUtilities.GetCardinality(new[] { "cardinality one", "cardinality zero_or_one" }));
    }

    [Theory]
    [InlineData("first")]
    [InlineData("first_or_default")]
    [InlineData("cardinality invalid")]
    public void GetCardinality_IgnoresUnsupportedCardinalityDirectives(string directive)
    {
        var cardinality = SqlUtilities.GetCardinality(new[] { directive });

        Assert.Equal(QueryCardinality.ZeroOrMore, cardinality);
    }
}
