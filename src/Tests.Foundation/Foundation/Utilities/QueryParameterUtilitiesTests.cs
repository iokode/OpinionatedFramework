using System;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Utilities;

namespace IOKode.OpinionatedFramework.Tests.Foundation.Foundation.Utilities;

public class QueryParameterUtilitiesTests
{
    private record Parameters
    {
        public required string Name { get; init; }
        [SetByMiddleware] public Guid? TenantId { get; init; }
        [SetByMiddleware] public string? UserName { get; init; }
    }

    [Theory]
    [InlineData("TenantId")]
    [InlineData("tenantId")]
    [InlineData("tenant_id")]
    public void HasParameter_ByPropertyOrSqlName_True(string name)
    {
        var parameters = new Parameters { Name = "name" };

        Assert.True(QueryParameterUtilities.HasParameter(parameters, name));
    }

    [Fact]
    public void HasParameter_UnknownOrNoParameters_False()
    {
        Assert.False(QueryParameterUtilities.HasParameter(new Parameters { Name = "name" }, "other"));
        Assert.False(QueryParameterUtilities.HasParameter(null, "tenant_id"));
    }

    [Fact]
    public void SetParameter_BySqlName_SetsTheProperty()
    {
        var parameters = new Parameters { Name = "name" };
        var tenantId = Guid.NewGuid();

        QueryParameterUtilities.SetParameter(parameters, "tenant_id", tenantId);
        QueryParameterUtilities.SetParameter(parameters, "Name", "other");

        Assert.Equal(tenantId, parameters.TenantId);
        Assert.Equal("other", parameters.Name);
    }

    [Fact]
    public void SetParameter_UnknownParameter_Throws()
    {
        var parameters = new Parameters { Name = "name" };

        Assert.Throws<ArgumentException>(() => QueryParameterUtilities.SetParameter(parameters, "other", 1));
    }

    [Fact]
    public void SetParameter_ValueOfAnotherType_Throws()
    {
        var parameters = new Parameters { Name = "name" };

        Assert.Throws<ArgumentException>(() => QueryParameterUtilities.SetParameter(parameters, "tenant_id", "not a guid"));
    }

    [Fact]
    public void SetParameter_ReadOnlyParameter_Throws()
    {
        var parameters = new { TenantId = Guid.Empty };

        Assert.Throws<InvalidOperationException>(() => QueryParameterUtilities.SetParameter(parameters, "tenant_id", Guid.NewGuid()));
    }

    [Fact]
    public void EnsureMiddlewareParametersAreSet_UnsetParameter_Throws()
    {
        var parameters = new Parameters { Name = "name", UserName = "user" };

        var exception = Assert.Throws<QueryParameterNotSetException>(() => QueryParameterUtilities.EnsureMiddlewareParametersAreSet(parameters));
        Assert.Equal(nameof(Parameters.TenantId), exception.ParameterName);
    }

    [Fact]
    public void EnsureMiddlewareParametersAreSet_AllSet_DoesNotThrow()
    {
        var parameters = new Parameters { Name = "name", TenantId = Guid.NewGuid(), UserName = "user" };

        QueryParameterUtilities.EnsureMiddlewareParametersAreSet(parameters);
        QueryParameterUtilities.EnsureMiddlewareParametersAreSet(new { Other = (string?) null });
        QueryParameterUtilities.EnsureMiddlewareParametersAreSet(null);
    }
}
