using System;
using System.Security.Cryptography;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.ContractImplementations.Aes256GcmModeEncrypter;

public class Aes256GcmModeEncrypter : IEncrypter
{
    private byte[] _key;

    public Aes256GcmModeEncrypter(byte[] key)
    {
        Ensure.Boolean.IsTrue(key.Length == 32)
            .ElseThrowsIllegalArgument("The key must be exactly 256 bits in length", nameof(key));

        _key = key;
    }

    public ReadOnlySpan<byte> Encrypt(ReadOnlySpan<byte> plainData)
    {
        byte[] nonce = _generateNonce();
        byte[] tag = _generateTag();
        var cipheredData = new byte[plainData.Length];
        var result = new byte[nonce.Length + cipheredData.Length + tag.Length];

        using var aes = new AesGcm(_key);
        aes.Encrypt(nonce, plainData, cipheredData, tag);

        nonce.CopyTo(result.AsSpan(0, nonce.Length));
        cipheredData.CopyTo(result.AsSpan(nonce.Length, cipheredData.Length));
        tag.CopyTo(result.AsSpan(nonce.Length + cipheredData.Length, tag.Length));

        return result;
    }

    public ReadOnlySpan<byte> Decrypt(ReadOnlySpan<byte> payload)
    {
        var nonce = payload[..AesGcm.NonceByteSizes.MaxSize];
        var tag = payload.Slice(payload.Length - AesGcm.TagByteSizes.MaxSize, AesGcm.TagByteSizes.MaxSize);
        var cipheredData = payload.Slice(AesGcm.NonceByteSizes.MaxSize,
            payload.Length - (AesGcm.NonceByteSizes.MaxSize + AesGcm.TagByteSizes.MaxSize));
        byte[] decryptedData = new byte[cipheredData.Length];

        using var aes = new AesGcm(_key);
        aes.Decrypt(nonce, cipheredData, tag, decryptedData, null);

        return decryptedData;
    }

    private byte[] _generateNonce()
    {
        return RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
    }

    private byte[] _generateTag()
    {
        return RandomNumberGenerator.GetBytes(AesGcm.TagByteSizes.MaxSize);
    }
}