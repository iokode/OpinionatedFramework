namespace IOKode.OpinionatedFramework.Contracts;

[Contract]
public interface IEncrypter
{
    public string Encrypt(string value);
    public string Decrypt(string payload);
}