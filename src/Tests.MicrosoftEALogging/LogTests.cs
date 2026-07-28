using System;
using System.Linq;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftEALogging;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MicrosoftEALogging;

public class LogTests
{
    private static TestLoggerProvider provider = null!;

    public LogTests()
    {
        Container.Advanced.ResetAsync().AsTask().GetAwaiter().GetResult();
        provider = new TestLoggerProvider();

        Container.Services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(provider);
        });
        Container.Services.AddMicrosoftExtensionsAbstractionsLogging();

        Container.Initialize();
    }

    [Fact]
    public void UsingFacade()
    {
        // Act
        Log.Info("Expected log message");

        // Assert
        var loggers = provider.GetLoggers();
        var loggerEntries = loggers.Last().LogEntries;
        var logEntry = loggerEntries.Last();

        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Expected log message", logEntry.Message);
        Assert.Equal(typeof(LogTests).FullName, logEntry.CategoryName);
    }

    [Fact]
    public void WithoutFacade()
    {
        // Arrange
        var logging = Locator.Resolve<ILogging>();

        // Act
        logging.Info("Expected log message");

        // Assert
        var loggers = provider.GetLoggers();
        var loggerEntries = loggers.Last().LogEntries;
        var logEntry = loggerEntries.Last();

        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Expected log message", logEntry.Message);
        Assert.Equal(typeof(LogTests).FullName, logEntry.CategoryName);
    }

    // The name derivation is delegated to the ILoggerFactory, so it follows the standard
    // Microsoft.Extensions.Logging convention: '.' as the nested-type delimiter and no generic parameters.
    [Theory]
    [InlineData(typeof(LogTests),
        "IOKode.OpinionatedFramework.Tests.MicrosoftEALogging.LogTests")]
    [InlineData(typeof(NestedCategory),
        "IOKode.OpinionatedFramework.Tests.MicrosoftEALogging.LogTests.NestedCategory")]
    [InlineData(typeof(GenericCategory<int>),
        "IOKode.OpinionatedFramework.Tests.MicrosoftEALogging.GenericCategory")]
    public void FromCategory_ByType_ProducesTypeDisplayName(Type categoryType, string expectedCategoryName)
    {
        // Arrange
        var logging = Locator.Resolve<ILogging>();

        // Act
        logging.FromCategory(categoryType).LogInformation("probe");

        // Assert
        var logEntry = provider.GetLoggers()
            .SelectMany(logger => logger.LogEntries)
            .Single(entry => entry.Message == "probe");
        Assert.Equal(expectedCategoryName, logEntry.CategoryName);
    }

    private class NestedCategory;
}

internal class GenericCategory<T>;
