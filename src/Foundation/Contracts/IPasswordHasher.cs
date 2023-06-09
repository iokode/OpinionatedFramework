using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("Password")]
public interface IPasswordHasher
{
    public string Hash(string plainText);
    public bool Check(string plainText, string hashedText);
}