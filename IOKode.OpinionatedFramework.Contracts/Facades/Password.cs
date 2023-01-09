using IOKode.OpinionatedFramework.Foundation;

namespace IOKode.OpinionatedFramework.Contracts.Facades;

public static class Password
{
    public static string Hash(string plainText)
    {
        return _getHasher().Hash(plainText);
    }

    public static bool Check(string plainText, string hashedPassword)
    {
        return _getHasher().Check(plainText, hashedPassword);
    }

    private static IPasswordHasher _getHasher()
    {
        return Locator.Resolve<IPasswordHasher>();
    }
}