using IOKode.OpinionatedFramework.Contracts;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Aes256GcmModeEncrypter;

public class Aes256GcmModeEncrypterTests
{
    private static readonly byte[] _aesKey =
    {
        212, 248, 83, 123, 229, 152, 249, 204, 132, 244, 85, 52, 91, 96, 183, 232, 192, 76, 113, 94, 186, 194, 157, 81,
        49, 132, 238, 55, 216, 207, 72, 32
    };

    [Fact]
    public void EncryptDecrypt_String()
    {
        IEncrypter encrypter = new ContractImplementations.Aes256GcmModeEncrypter.Aes256GcmModeEncrypter(_aesKey);
        const string originalText = "Hello, world!";

        var encryptedText = encrypter.Encrypt(originalText);
        string decryptedText = encrypter.DecryptString(encryptedText);

        Assert.Equal(originalText, decryptedText);
    }

    [Fact]
    public void EncryptDecrypt_Bytes()
    {
        IEncrypter encrypter = new ContractImplementations.Aes256GcmModeEncrypter.Aes256GcmModeEncrypter(_aesKey);
        byte[] originalBytes = { 1, 2, 3, 4, 5, 6, 7, 8 };
        
        var encryptedBytes = encrypter.Encrypt(originalBytes);
        var decryptedBytes = encrypter.Decrypt(encryptedBytes);

        Assert.Equal(originalBytes, decryptedBytes.ToArray());
        Assert.NotEqual(originalBytes, encryptedBytes.ToArray());
    }
}