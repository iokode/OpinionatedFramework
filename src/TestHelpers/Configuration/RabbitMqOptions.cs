namespace IOKode.OpinionatedFramework.TestHelpers.Configuration;

public class RabbitMqOptions
{
    public required string Image { get; set; }

    public required string Tag { get; set; }

    public required string ContainerName { get; set; }

    public required string HostPort { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }

    public string ImageWithTag => $"{Image}:{Tag}";

    public static RabbitMqOptions Default = new()
    {
        Image = "rabbitmq",
        Tag = "4.1.8-alpine",
        ContainerName = "oftest_rabbitmq",
        HostPort = "5672",
        Username = "guest",
        Password = "guest"
    };
}
