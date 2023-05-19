using System.IO;
using System.Text;

namespace IOKode.OpinionatedFramework.Contracts;

/// <summary>
/// Provides a blueprint for cryptographic operations.
/// </summary>
[AddToFacade("Crypt")]
public interface IEncrypter
{
    /// <summary>
    /// Encrypts the given plain data.
    /// </summary>
    /// <param name="plainData">The plain data to encrypt.</param>
    /// <returns>A Stream containing the encrypted data.</returns>
    public Stream Encrypt(Stream plainData);

    /// <summary>
    /// Decrypts the given payload.
    /// </summary>
    /// <param name="payload">The payload to decrypt.</param>
    /// <returns>A Stream containing the decrypted data.</returns>
    public Stream Decrypt(Stream payload);

    /// <summary>
    /// Encrypts the given plain data.
    /// </summary>
    /// <param name="plainData">The plain data to encrypt.</param>
    /// <returns>A Stream containing the encrypted data.</returns>
    public Stream Encrypt(byte[] plainData)
    {
        using var plainDataStream = new MemoryStream(plainData);
        return Encrypt(plainDataStream);
    }

    /// <summary>
    /// Encrypts the given plain text using the UTF-8 encoding.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>A Stream containing the encrypted data.</returns>
    public Stream Encrypt(string plainText)
    {
        return Encrypt(plainText, Encoding.UTF8);
    }

    /// <summary>
    /// Encrypts the given plain text using the specified encoding.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>A Stream containing the encrypted data.</returns>
    public Stream Encrypt(string plainText, Encoding encoding)
    {
        byte[] plainData = encoding.GetBytes(plainText);
        return Encrypt(plainData);
    }

    /// <summary>
    /// Decrypts the given payload into a byte array.
    /// </summary>
    /// <param name="payload">The payload to decrypt.</param>
    /// <returns>A byte array containing the decrypted data.</returns>
    public byte[] DecryptByteArray(Stream payload)
    {
        using var decryptedStream = Decrypt(payload);
        using var memoryStream = new MemoryStream();
        decryptedStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Decrypts the given payload into a string using UTF-8 encoding.
    /// </summary>
    /// <param name="payload">The payload to decrypt.</param>
    /// <returns>A string containing the decrypted data.</returns>
    public string DecryptString(Stream payload)
    {
        return DecryptString(payload, Encoding.UTF8);
    }

    /// <summary>
    /// Decrypts the given payload into a string using the specified encoding.
    /// </summary>
    /// <param name="payload">The payload to decrypt.</param>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>A string containing the decrypted data.</returns>
    public string DecryptString(Stream payload, Encoding encoding)
    {
        using var decryptedStream = Decrypt(payload);
        using var reader = new StreamReader(decryptedStream, encoding);
        return reader.ReadToEnd();
    }
}