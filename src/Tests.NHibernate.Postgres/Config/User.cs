using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Ensuring.Ensurers;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public class User : Entity
{
#pragma warning disable CS0414
    private string id = null!;
#pragma warning restore CS0414

    public required string Username
    {
        get;
        set
        {
            Ensure.String.Alphanumeric(value, AlphaOptions.Default).ElseThrowsIllegalArgument(nameof(value), "Invalid username.");
            field = value;
        }
    }

    public required string EmailAddress
    {
        get;
        set
        {
            Ensure.String.Email(value).ElseThrowsIllegalArgument(nameof(value), "Invalid email address.");
            field = value;
        }
    }

    public required bool IsActive { get; set; }
}