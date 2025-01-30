using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.Ensuring.Ensurers;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public class User : Entity
{
    private string id;
    private string username;
    private string emailAddress;
    private bool isActive;

    public User()
    {
    }
    
    public required string Username
    {
        get => this.username;
        set
        {
            Ensure.String.Alphanumeric(value, AlphaOptions.Default).ElseThrowsIllegalArgument(nameof(value), "Invalid username.");
            this.username = value;
        }
    }

    public required string EmailAddress
    {
        get => this.emailAddress;
        set
        {
            Ensure.String.Email(value).ElseThrowsIllegalArgument(nameof(value), "Invalid email address.");
            this.emailAddress = value;
        }
    }

    public required bool IsActive
    {
        get => this.isActive;
        set => this.isActive = value;
    }
}