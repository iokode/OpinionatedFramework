using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("Job")]
public interface IJobScheduler
{
    public Task ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken);
}