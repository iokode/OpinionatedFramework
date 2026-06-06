using System;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.TestHelpers;

public class XUnitLogging(ITestOutputHelper testOutputHelper) : ILogging
{
    public ILogger FromCategory(string category)
    {
        return XUnitLogger.CreateLogger(testOutputHelper, category);
    }

    public ILogger FromCategory(Type categoryType)
    {
        return XUnitLogger.CreateLogger(testOutputHelper, categoryType);
    }
}
