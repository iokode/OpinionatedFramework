using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Foundation;
using IOKode.OpinionatedFramework.Foundation.Jobs;

namespace IOKode.OpinionatedFramework.Contracts;

[Contract]
public interface IJobScheduler
{
    public Task ScheduleAsync(IJob job, CronExpression interval, CancellationToken cancellationToken);
}