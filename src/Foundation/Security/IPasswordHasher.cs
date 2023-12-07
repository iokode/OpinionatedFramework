using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Security;

[AddToFacade("Password")]
public interface IPasswordHasher
{
    public string Hash(string plainText);
    public bool Check(string plainText, string hashedText);
}