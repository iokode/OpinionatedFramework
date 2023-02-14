using System;
using IOKode.OpinionatedFramework.Contracts;

namespace IOKode.OpinionatedFramework.ConfigureApplication;

public static class Configure
{
    public static void UnitOfWorkCreator(Func<IUnitOfWorkFactory> getFactoryFunction)
    {
        Container.Register(getFactoryFunction);
    }

    public static void EventDispatcher(Func<IEventDispatcher> getDispatcherFunction)
    {
        Container.Register(getDispatcherFunction);
    }

    public static void JobScheduler(Func<IJobScheduler> getSchedulerFunction)
    {
        Container.Register(getSchedulerFunction);
    }

    public static void JobEnqueuer(Func<IJobEnqueuer> getEnqueuerFunction)
    {
        Container.Register(getEnqueuerFunction);
    }
}