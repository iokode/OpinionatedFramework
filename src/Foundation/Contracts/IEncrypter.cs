using System;
using System.IO;
using System.Text;
using IOKode.OpinionatedFramework.Facades;

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
    /// <returns>The encrypted data.</returns>
    public ReadOnlySpan<byte> Encrypt(ReadOnlySpan<byte> plainData);

    /// <summary>
    /// Decrypts the given payload.
    /// </summary>
    /// <param name="payload">The payload to decrypt.</param>
    /// <returns>The decrypted data.</returns>
    public ReadOnlySpan<byte> Decrypt(ReadOnlySpan<byte> payload);

    /// <summary>
    /// Encrypts the given plain data.
    /// </summary>
    /// <remarks>
    /// This is a default implementation which creates an span from the byte array and then calls <see cref="Encrypt(System.ReadOnlySpan{byte})"/>.
    /// </remarks>
    /// <param name="plainData">The plain data to encrypt.</param>
    /// <returns>The encrypted data.</returns>
    public ReadOnlySpan<byte> Encrypt(byte[] plainData)
    {
        return Encrypt((ReadOnlySpan<byte>)plainData);
    }

    /// <summary>
    /// Encrypts the given plain data from the specified stream.
    /// </summary>
    /// <remarks>
    /// This is a default implementation which copies the data from the stream into a byte array and then calls <see cref="Encrypt(byte[])"/>.
    /// </remarks>
    /// <param name="plainData">The stream containing the plain data to encrypt.</param>
    /// <returns>The encrypted data.</returns>
    public ReadOnlySpan<byte> Encrypt(Stream plainData)
    {
        using var ms = new MemoryStream();
        plainData.CopyTo(ms);
        return Encrypt(ms.ToArray());
    }

    /// <summary>
    /// Encrypts the specified plain text using UTF-8 encoding.
    /// </summary>
    /// <remarks>
    /// This is a default implementation which calls <see cref="Encrypt(string, Encoding)"/> with UTF-8 as the encoding.
    /// </remarks>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted data.</returns>
    public ReadOnlySpan<byte> Encrypt(string plainText)
    {
        return Encrypt(plainText, Encoding.UTF8);
    }

    /// <summary>
    /// Encrypts the specified plain text, using the specified encoding to convert the string to a byte array.
    /// </summary>
    /// <remarks>
    /// This is a default implementation which encodes the string into byte array and then calls <see cref="Encrypt(byte[])"/>.
    /// </remarks>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>The encrypted data.</returns>
    public ReadOnlySpan<byte> Encrypt(string plainText, Encoding encoding)
    {
        byte[] plainData = encoding.GetBytes(plainText);
        return Encrypt(plainData);
    }

    /// <summary>
    /// Decrypts the specified payload into a string, using UTF-8 encoding.
    /// </summary>
    /// <remarks>
    /// This is a default implementation which calls <see cref="DecryptString(ReadOnlySpan{byte}, Encoding)"/> with UTF-8 as the encoding.
    /// </remarks>
    /// <param name="payload">The payload to decrypt.</param>
    /// <returns>The decrypted string.</returns>
    public string DecryptString(ReadOnlySpan<byte> payload)
    {
        return DecryptString(payload, Encoding.UTF8);
    }
    
    /// <summary>
    /// Decrypts the specified payload into a string, using the specified encoding.
    /// </summary>
    /// <remarks>
    /// This is a default implementation which decodes the string into byte array and then calls <see cref="Decrypt(ReadOnlySpan{byte})"/>.
    /// </remarks>
    /// <param name="payload">The payload to decrypt.</param>
    /// <param name="encoding">The encoding to use when converting the decrypted bytes to a string.</param>
    /// <returns>The decrypted string.</returns>
    public string DecryptString(ReadOnlySpan<byte> payload, Encoding encoding)
    {
        var decryptedPayload = Decrypt(payload);
        string decryptedString = encoding.GetString(decryptedPayload);
        return decryptedString;
    }

    /// <summary>
    /// Decrypts the specified payload into a Stream.
    /// </summary>
    /// <remarks>
    /// This is a default implementation which copies the decrypted data into a <see cref="MemoryStream"/>.
    /// </remarks>
    /// <param name="payload">The payload to decrypt.</param>
    /// <returns>The decrypted data in a stream.</returns>
    public Stream DecryptStream(ReadOnlySpan<byte> payload)
    {
        var decryptedPayload = Decrypt(payload);
        var ms = new MemoryStream(decryptedPayload.ToArray());
        return ms;
    }
}