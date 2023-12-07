using IOKode.OpinionatedFramework.Security;

namespace IOKode.OpinionatedFramework.ContractImplementations.Bcrypt;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainText)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainText);
    }

    public bool Check(string plainText, string hashedText)
    {
        return BCrypt.Net.BCrypt.Verify(plainText, hashedText);
    }
}