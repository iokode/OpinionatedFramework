using MailKit.Security;

namespace IOKode.OpinionatedFramework.ContractImplementations.MailKit;

public class MailKitOptions
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public bool Authenticate { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public SecureSocketOptions Secure { get; set; }
}