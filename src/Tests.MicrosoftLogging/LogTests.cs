using System.Linq;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MicrosoftLogging;

public class LogTests
{
    private static TestLoggerProvider provider = null!;
    
    public LogTests()
    {
        if (Container.IsInitialized)
        {
            return;
        }
        
        var testLoggerProvider = new TestLoggerProvider();
        Container.Services.AddMicrosoftLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(testLoggerProvider);
        });

        Container.Initialize();
        provider = testLoggerProvider;
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
}