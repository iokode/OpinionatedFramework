namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("Crypt")]
public interface IEncrypter
{
    public string Encrypt(string value);
    public string Decrypt(string payload);
}