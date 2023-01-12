using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Contracts.Persistence;
using IOKode.OpinionatedFramework.Foundation;
using IOKode.OpinionatedFramework.Foundation.Jobs;

namespace IOKode.OpinionatedFramework.ConfigureApplication;

public static class Configuration
{
    public static void UnitOfWorkCreator(Func<IUnitOfWorkFactory> getFactoryFunction)
    {
        LocatorConfiguration.Register(getFactoryFunction);
    }

    public static void EventDispatcher(Func<IEventDispatcher> getDispatcherFunction)
    {
        LocatorConfiguration.Register(getDispatcherFunction);
    }

    public static void JobScheduler(Func<IJobScheduler> getSchedulerFunction)
    {
        LocatorConfiguration.Register(getSchedulerFunction);
    }

    public static void JobQueuing(Func<IJob, CancellationToken, Task> enqueueFunction)
    {
        var field = typeof(IJob).GetField("_enqueueFunction",
            BindingFlags.Static | BindingFlags.NonPublic);

        field!.SetValue(null, enqueueFunction);
    }

    public static void JobQueuingWithDelay(Func<IJob, TimeSpan, CancellationToken, Task> enqueueFunction)
    {
        var field = typeof(IJob).GetField("_enqueueWithDelayFunction",
            BindingFlags.Static | BindingFlags.NonPublic);

        field!.SetValue(null, enqueueFunction);
    }
}