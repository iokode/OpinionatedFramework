namespace IOKode.OpinionatedFramework.Tests.MicrosoftConfiguration;

public class User
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string[]? Permissions { get; init; }
    public User? Friend { get; init; }
}

public record Computer
{
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public decimal? Price { get; init; }
}