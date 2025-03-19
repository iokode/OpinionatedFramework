using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Jobs;

public abstract class Job
{
    public abstract Task ExecuteAsync(IJobExecutionContext context);
}