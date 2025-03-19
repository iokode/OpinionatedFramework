using System.Linq;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Emailing.Extensions;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.Emailing;

public class SendEmailJob(Email email) : Job
{
    public override async Task ExecuteAsync(IJobExecutionContext context)
    {
        await email.SendAsync(context.CancellationToken);
    }
}

public class SendEmailJobCreator(Email Email) : JobCreator<SendEmailJob>
{
    public override SendEmailJob CreateJob() => new(Email);

    public override string GetJobName() => $"Send '{Email.Subject}' to {Email.To.First()} via email.";
}