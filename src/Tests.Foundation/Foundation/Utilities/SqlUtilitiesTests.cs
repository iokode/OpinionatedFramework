using System;
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

    [Theory]
    [InlineData(QueryCardinality.ZeroOrMore, null)]
    [InlineData(QueryCardinality.One, "single")]
    [InlineData(QueryCardinality.ZeroOrOne, "single_or_default")]
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
        Assert.Throws<InvalidOperationException>(() => SqlUtilities.GetCardinality(new[] { "single", "single_or_default" }));
    }

    [Theory]
    [InlineData("first")]
    [InlineData("first_or_default")]
    public void GetCardinality_IgnoresUnsupportedCardinalityDirectives(string directive)
    {
        var cardinality = SqlUtilities.GetCardinality(new[] { directive });

        Assert.Equal(QueryCardinality.ZeroOrMore, cardinality);
    }
}
