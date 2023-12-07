using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Jobs;

[AddToFacade("Job")]
public interface IJobScheduler
{
    public Task ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken);
}