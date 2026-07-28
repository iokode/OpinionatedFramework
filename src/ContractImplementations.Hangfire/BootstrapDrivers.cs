using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hangfire;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Jobs;
using Microsoft.Extensions.Configuration;

[assembly: BootstrapDriver<IJobEnqueuer, HangfireJobEnqueuerBootstrapDriver>("JobEnqueuer", "hangfire")]
[assembly: BootstrapDriver<IJobScheduler, HangfireJobSchedulerBootstrapDriver>("JobScheduler", "hangfire")]

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

public sealed class HangfireJobEnqueuerBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return HangfireBootstrapRegistration.Validate(context);
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddHangfireJobEnqueuer();
        HangfireBootstrapRegistration.Register(context);
    }
}

public sealed class HangfireJobSchedulerBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return HangfireBootstrapRegistration.Validate(context);
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddHangfireJobScheduler();
        HangfireBootstrapRegistration.Register(context);
    }
}

/// <summary>
/// Configures Hangfire and the worker once for every contract the driver is selected for.
/// </summary>
/// <remarks>
/// The job enqueuer and the job scheduler are separate contracts backed by one Hangfire setup, so the storage is
/// configured and the worker is registered through shared state instead of once per contract.
/// </remarks>
internal static class HangfireBootstrapRegistration
{
    /// <summary>
    /// The root configuration section holding the Hangfire setup, outside the <c>OpinionatedFramework</c> section.
    /// </summary>
    private const string ConfigurationSectionKey = "Hangfire";

    private const string ConfigurationStateKey = "OpinionatedFramework.Hangfire.Configuration";
    private const string OptionsStateKey = "OpinionatedFramework.Hangfire.Options";
    private const string ValidationStateKey = "OpinionatedFramework.Hangfire.Validation";
    private const string RegistrationStateKey = "OpinionatedFramework.Hangfire";

    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var state = GetValidationState(context);
        if (state.ValidationReturned)
        {
            return BootstrapValidationResult.Success;
        }

        state.ValidationReturned = true;
        return state.ValidationResult;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.GetOrAddSharedState(RegistrationStateKey, () =>
        {
            var options = GetOptions(context);

            // The enqueuer and the scheduler resolve the storage through JobStorage.Current, so Hangfire is
            // configured whether or not this process also runs a worker.
            options.ApplyHangfireConfigurators(GlobalConfiguration.Configuration);

            var configurationState = GetConfigurationState(context);
            if (configurationState.StartWorker)
            {
                var serverOptions = new BackgroundJobServerOptions();
                configurationState.ApplyTo(serverOptions);

                // Applied last, so a delegate overrides what configuration set while leaving untouched
                // settings at their configured value.
                options.ApplyServerConfigurators(serverOptions);

                context.Services.AddHangfireWorker(serverOptions);
            }

            return new RegistrationState();
        });
    }

    private static ValidationState GetValidationState(BootstrapDriverContext context)
    {
        return context.GetOrAddSharedState(ValidationStateKey, () =>
        {
            var errors = new List<BootstrapValidationError>(GetConfigurationState(context).Errors);

            // Checked from state rather than by applying the configurators, because validation runs before the
            // bootstrap is committed and must not mutate the process-wide Hangfire configuration.
            if (!GetOptions(context).HasHangfireConfigurators && !IsJobStorageInitialized())
            {
                errors.Add(new BootstrapValidationError(
                    ConfigurationSectionKey,
                    "A Hangfire storage is required. Supply it with options.Hangfire(hangfire => " +
                    "hangfire.ConfigureHangfire(configuration => configuration.UsePostgreSqlStorage(...)))."));
            }

            return new ValidationState(new BootstrapValidationResult(errors));
        });
    }

    private static HangfireOptions GetOptions(BootstrapDriverContext context)
    {
        return context.GetOrAddSharedState(OptionsStateKey, () =>
        {
            var options = new HangfireOptions();
            context.GetOptionsConfigurator<HangfireOptions>()?.Invoke(options);
            return options;
        });
    }

    private static HangfireConfigurationState GetConfigurationState(BootstrapDriverContext context)
    {
        return context.GetOrAddSharedState(ConfigurationStateKey, () =>
        {
            var errors = new List<BootstrapValidationError>();

            // The Hangfire setup is not a driver, so it owns a root section instead of living under the
            // OpinionatedFramework section, which holds the contract slots and nothing else.
            var section = context.Configuration.GetSection(ConfigurationSectionKey);

            return new HangfireConfigurationState(
                ReadStartWorker(section, errors),
                ReadQueues(section, errors),
                ReadWorkerCount(section, errors),
                ReadShutdownTimeout(section, errors),
                errors);
        });
    }

    private static bool ReadStartWorker(IConfigurationSection section, List<BootstrapValidationError> errors)
    {
        var configuredValue = section["StartWorker"];
        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            return false;
        }

        if (bool.TryParse(configuredValue, out var startWorker))
        {
            return startWorker;
        }

        errors.Add(new BootstrapValidationError($"{section.Path}:StartWorker", "The value must be a boolean."));
        return false;
    }

    private static string[]? ReadQueues(IConfigurationSection section, List<BootstrapValidationError> errors)
    {
        var queueSections = section.GetSection("Queues").GetChildren().ToArray();
        if (queueSections.Length == 0)
        {
            return null;
        }

        var queues = new List<string>(queueSections.Length);
        foreach (var queueSection in queueSections)
        {
            if (string.IsNullOrWhiteSpace(queueSection.Value))
            {
                errors.Add(new BootstrapValidationError(queueSection.Path, "A value is required."));
                continue;
            }

            queues.Add(queueSection.Value);
        }

        return queues.ToArray();
    }

    private static int? ReadWorkerCount(IConfigurationSection section, List<BootstrapValidationError> errors)
    {
        var configuredValue = section["WorkerCount"];
        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            return null;
        }

        if (int.TryParse(configuredValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var workerCount)
            && workerCount > 0)
        {
            return workerCount;
        }

        errors.Add(new BootstrapValidationError(
            $"{section.Path}:WorkerCount",
            "The value must be a positive integer."));
        return null;
    }

    private static TimeSpan? ReadShutdownTimeout(IConfigurationSection section, List<BootstrapValidationError> errors)
    {
        var configuredValue = section["ShutdownTimeout"];
        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            return null;
        }

        if (TimeSpan.TryParse(configuredValue, CultureInfo.InvariantCulture, out var shutdownTimeout)
            && shutdownTimeout > TimeSpan.Zero)
        {
            return shutdownTimeout;
        }

        errors.Add(new BootstrapValidationError(
            $"{section.Path}:ShutdownTimeout",
            "The value must be a positive time span, such as 00:00:30."));
        return null;
    }

    private static bool IsJobStorageInitialized()
    {
        try
        {
            _ = JobStorage.Current;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private sealed class RegistrationState;

    private sealed class ValidationState(BootstrapValidationResult validationResult)
    {
        public BootstrapValidationResult ValidationResult { get; } = validationResult;
        public bool ValidationReturned { get; set; }
    }

    private sealed class HangfireConfigurationState(
        bool startWorker,
        string[]? queues,
        int? workerCount,
        TimeSpan? shutdownTimeout,
        IReadOnlyCollection<BootstrapValidationError> errors)
    {
        public bool StartWorker { get; } = startWorker;
        public IReadOnlyCollection<BootstrapValidationError> Errors { get; } = errors;

        /// <summary>
        /// Applies the configured values, leaving the Hangfire default in place for every absent key.
        /// </summary>
        public void ApplyTo(BackgroundJobServerOptions serverOptions)
        {
            if (queues is not null)
            {
                serverOptions.Queues = queues;
            }

            if (workerCount is not null)
            {
                serverOptions.WorkerCount = workerCount.Value;
            }

            if (shutdownTimeout is not null)
            {
                serverOptions.ShutdownTimeout = shutdownTimeout.Value;
            }
        }
    }
}
