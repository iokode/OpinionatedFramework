using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.Aes256GcmModeEncrypter;
using IOKode.OpinionatedFramework.ContractImplementations.Bcrypt;
using IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;
using IOKode.OpinionatedFramework.ContractImplementations.LoggerEmail;
using IOKode.OpinionatedFramework.ContractImplementations.MailKit;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftConfiguration;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;
using IOKode.OpinionatedFramework.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.DefaultBootstrapping;

public static class DefaultBootstrapping
{
    public static void Bootstrap(IConfiguration configuration)
    {
        Container.Services.AddTransient<IEncrypter, Aes256GcmModeEncrypter>();
        Container.Services.AddTransient<IPasswordHasher, BcryptPasswordHasher>();

        Container.Services.AddDefaultCommandExecutor();

        if (configuration["Email:Logger"] == "true")
        {
            Container.Services.AddLoggerEmail();
        }
        else
        {
            Container.Services.AddMailKit(configuration.GetSection("Email:Smtp"));
        }

        Container.Services.AddMicrosoftLogging(options =>
        {
            var minimumLevel = configuration["Logging:MinimumLevel"] switch
            {
                "Trace" => LogLevel.Trace,
                "Debug" => LogLevel.Debug,
                "Info" or "Information" => LogLevel.Information,
                "Warn" or "Warning" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                "Critical" => LogLevel.Critical,
                "None" => LogLevel.None,
                _ => LogLevel.Trace
            };
            options.SetMinimumLevel(minimumLevel);
            options.AddConsole();
        });

        Container.Services.AddMicrosoftConfiguration(configuration);
    }
}