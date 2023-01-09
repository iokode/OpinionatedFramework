namespace IOKode.OpinionatedFramework.Contracts;

[Contract]
public interface IPasswordHasher
{
    public string Hash(string plainText);
    public bool Check(string plainText, string hashedText);
}