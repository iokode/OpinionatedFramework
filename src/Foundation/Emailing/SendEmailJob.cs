using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Emailing.Extensions;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.Emailing;

public class SendEmailJob(Email email) : IJob
{
    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        await email.SendAsync(cancellationToken);
    }
}

public record SendEmailJobArguments(Email Email) : JobArguments<SendEmailJob>
{
    public override SendEmailJob CreateJob() => new(Email);
}