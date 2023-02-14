using System;
using IOKode.OpinionatedFramework.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ConfigureApplication;

public static class EmailConfiguration
{
    public static void RegisterSender(Func<IEmailSender> getSenderFunction)
    {
        Container.Register(getSenderFunction);
    }

    public static void RegisterSender<TSenderType>() where TSenderType : IEmailSender
    {
        Container.Register<IEmailSender>(() => Activator.CreateInstance<TSenderType>());
    }

    public static void RegisterSender<TSenderType>(IServiceProvider serviceProvider) where TSenderType : IEmailSender
    {
        Container.Register<IEmailSender>(() => ActivatorUtilities.CreateInstance<TSenderType>(serviceProvider));
    }
}