namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("Encrypt")]
public interface IEncrypter
{
    public string Encrypt(string value);
    public string Decrypt(string payload);
}