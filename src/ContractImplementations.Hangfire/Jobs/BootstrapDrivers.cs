using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Jobs;

[assembly: BootstrapDriver<IJobEnqueuer,
    IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs.HangfireJobEnqueuerBootstrapDriver>(
    "JobEnqueuer", "hangfire")]
[assembly: BootstrapDriver<IJobScheduler,
    IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs.HangfireJobSchedulerBootstrapDriver>(
    "JobScheduler", "hangfire")]

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public sealed class HangfireJobEnqueuerBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return HangfireWorkerBootstrapRegistration.Validate(context);
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddHangfireJobEnqueuer();
        HangfireWorkerBootstrapRegistration.Register(context);
    }
}

public sealed class HangfireJobSchedulerBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return HangfireWorkerBootstrapRegistration.Validate(context);
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddHangfireJobScheduler();
        HangfireWorkerBootstrapRegistration.Register(context);
    }
}

internal static class HangfireWorkerBootstrapRegistration
{
    private const string ConfigurationStateKey = "OpinionatedFramework.Hangfire.Worker.Configuration";
    private const string RegistrationStateKey = "OpinionatedFramework.Hangfire.Worker";

    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var state = GetConfigurationState(context);
        if (state.ValidationReturned)
        {
            return BootstrapValidationResult.Success;
        }

        state.ValidationReturned = true;
        return state.ValidationResult;
    }

    public static void Register(BootstrapDriverContext context)
    {
        if (!GetConfigurationState(context).StartWorker)
        {
            return;
        }

        context.GetOrAddSharedState(RegistrationStateKey, () =>
        {
            context.Services.AddHangfireWorker();
            return new WorkerRegistrationState();
        });
    }

    private static WorkerConfigurationState GetConfigurationState(BootstrapDriverContext context)
    {
        return context.GetOrAddSharedState(ConfigurationStateKey, () =>
        {
            var configurationPath = $"{context.FrameworkConfiguration.Path}:Hangfire:StartWorker";
            var configuredValue = context.FrameworkConfiguration["Hangfire:StartWorker"];
            if (string.IsNullOrWhiteSpace(configuredValue))
            {
                return new WorkerConfigurationState(false, BootstrapValidationResult.Success);
            }

            return bool.TryParse(configuredValue, out var startWorker)
                ? new WorkerConfigurationState(startWorker, BootstrapValidationResult.Success)
                : new WorkerConfigurationState(false, BootstrapValidationResult.Failure(
                    new BootstrapValidationError(configurationPath, "The value must be a boolean.")));
        });
    }

    private sealed class WorkerRegistrationState;

    private sealed class WorkerConfigurationState(
        bool startWorker,
        BootstrapValidationResult validationResult)
    {
        public bool StartWorker { get; } = startWorker;
        public BootstrapValidationResult ValidationResult { get; } = validationResult;
        public bool ValidationReturned { get; set; }
    }
}
