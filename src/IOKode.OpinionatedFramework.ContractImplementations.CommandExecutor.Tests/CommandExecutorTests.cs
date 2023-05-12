using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Foundation;
using IOKode.OpinionatedFramework.Foundation.Commands;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor.Tests;

public class CommandExecutorTests
{
    public class Service
    {
    }

    [Fact]
    public async Task CommandExecutor_InvokesCommandWithScopedService_SameServiceIsResolved()
    {
        // Arrange
        Container.Services.AddScoped<Service>();
        Container.Services.AddSingleton<ICommandExecutor, CommandExecutor>();
        Container.Initialize();

        // Act
        var command = new SampleCommand();
        var servicesFrom1stExecution = await Facades.Command.InvokeAsync<SampleCommand, (Service, Service)>(command);
        var servicesFrom2ndExecution = await Facades.Command.InvokeAsync<SampleCommand, (Service, Service)>(command);

        // Assert
        Assert.Same(servicesFrom1stExecution.Item1, servicesFrom1stExecution.Item2);
        Assert.Same(servicesFrom2ndExecution.Item1, servicesFrom2ndExecution.Item2);
        Assert.NotSame(servicesFrom1stExecution.Item1, servicesFrom2ndExecution.Item1);
    }

    public class SampleCommand : Command<(Service, Service)>
    {
        public override Task<(Service, Service)> ExecuteAsync()
        {
            var service = Locator.Resolve<Service>();
            var service2 = Locator.Resolve<Service>();

            return Task.FromResult((service, service2));
        }
    }
}