using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Security;
using Microsoft.Extensions.DependencyInjection;

[assembly: BootstrapDriver<IPasswordHasher,
    IOKode.OpinionatedFramework.ContractImplementations.Bcrypt.BcryptBootstrapDriver>(
    "PasswordHashing", "bcrypt", true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.Bcrypt;

public sealed class BcryptBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return BootstrapValidationResult.Success;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddTransient<IPasswordHasher, BcryptPasswordHasher>();
    }
}
