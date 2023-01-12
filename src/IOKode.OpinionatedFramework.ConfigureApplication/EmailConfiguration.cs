using System;
using IOKode.OpinionatedFramework.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ConfigureApplication;

public static class EmailConfiguration
{
    public static void RegisterSender(Func<IEmailSender> getSenderFunction)
    {
        LocatorConfiguration.Register(getSenderFunction);
    }

    public static void RegisterSender<TSenderType>() where TSenderType : IEmailSender
    {
        LocatorConfiguration.Register<IEmailSender>(() => Activator.CreateInstance<TSenderType>());
    }

    public static void RegisterSender<TSenderType>(IServiceProvider serviceProvider) where TSenderType : IEmailSender
    {
        LocatorConfiguration.Register<IEmailSender>(() => ActivatorUtilities.CreateInstance<TSenderType>(serviceProvider));
    }
}