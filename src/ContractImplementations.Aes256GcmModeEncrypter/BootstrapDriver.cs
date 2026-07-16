using System;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.Security;
using Microsoft.Extensions.DependencyInjection;

[assembly: BootstrapDriver<IEncrypter,
    IOKode.OpinionatedFramework.ContractImplementations.Aes256GcmModeEncrypter.Aes256GcmBootstrapDriver>(
    "Encryption", "aes-256-gcm")]

namespace IOKode.OpinionatedFramework.ContractImplementations.Aes256GcmModeEncrypter;

public sealed class Aes256GcmBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var keyPath = $"{context.DriverConfiguration.Path}:Key";
        var configuredKey = context.DriverConfiguration["Key"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            return BootstrapValidationResult.Failure(
                new BootstrapValidationError(keyPath, "A value is required."));
        }

        byte[] key;
        try
        {
            key = Convert.FromBase64String(configuredKey);
        }
        catch (FormatException)
        {
            return BootstrapValidationResult.Failure(
                new BootstrapValidationError(keyPath, "The value must be valid Base64."));
        }

        return key.Length == 32
            ? BootstrapValidationResult.Success
            : BootstrapValidationResult.Failure(
                new BootstrapValidationError(keyPath, "The decoded key must contain exactly 32 bytes."));
    }

    public static void Register(BootstrapDriverContext context)
    {
        var key = Convert.FromBase64String(context.DriverConfiguration["Key"]!);
        context.Services.AddSingleton<IEncrypter>(new Aes256GcmModeEncrypter(key));
    }
}
