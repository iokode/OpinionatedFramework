using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire.Config;

public abstract class JobsTestsBase
{
    protected JobsTestsBase(JobsTestsFixture fixture, ITestOutputHelper output)
    {
        fixture.TestOutputHelperFactory = () => output;
    }
}