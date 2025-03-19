using System.Linq;
using Hangfire.Server;
using IOKode.OpinionatedFramework.Emailing;

namespace IOKode.OpinionatedFramework.Tests.Hangfire.Config;

public class JobsChecker : IServerFilter
{
    public void OnPerforming(PerformingContext context)
    {
    }

    public void OnPerformed(PerformedContext context)
    {
        CheckEmailSend(context);
    }

    private void CheckEmailSend(PerformedContext context)
    {
        var jobCreator = context.BackgroundJob.Job.Args.Cast<SendEmailJobCreator>().FirstOrDefault();
        if (jobCreator is {Email.Subject: "Email sending test."})
        {
            FoundationJobsTest.IsExecutedSendEmailJob = true;
        }
    }
}