namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;

public record ControllersStrategy;

public record SingleControllerStrategy : ControllersStrategy
{
    public string Route { get; init; } = "api";
    public string HttpMethod { get; init; } = "post";
}