using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Foundation.Jobs;

public interface IJob
{
    public Task InvokeAsync(CancellationToken cancellationToken);
}