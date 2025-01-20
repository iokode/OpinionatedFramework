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
        var sendEmailJobArguments = context.BackgroundJob.Job.Args.Cast<SendEmailJobArguments>().FirstOrDefault();
        if (sendEmailJobArguments is {Email.Subject: "Email sending test."})
        {
            FoundationJobsTest.IsExecutedSendEmailJob = true;
        }
    }
}