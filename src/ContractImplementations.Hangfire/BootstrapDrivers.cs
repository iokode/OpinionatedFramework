using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hangfire;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Jobs;
using Microsoft.Extensions.Configuration;

[assembly: BootstrapDriver<IJobEnqueuer, HangfireJobEnqueuerBootstrapDriver>("JobEnqueuer", "Hangfire")]
[assembly: BootstrapDriver<IJobScheduler, HangfireJobSchedulerBootstrapDriver>("JobScheduler", "Hangfire")]

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
/// Configures Hangfire and its background job servers once for every contract the driver is selected for.
/// </summary>
/// <remarks>
/// The job enqueuer and the job scheduler are separate contracts backed by one Hangfire setup, so the storage is
/// configured and the servers are registered through shared state instead of once per contract.
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

            // Registers the whole Hangfire surface, including IGlobalConfiguration, JobStorage and the
            // dashboard routes, so an application needs no Hangfire registration of its own.
            context.Services.AddHangfire(options.ApplyHangfireConfigurators);

            foreach (var server in GetConfigurationState(context).Servers)
            {
                context.Services.AddHangfireServer(serverOptions =>
                {
                    server.ApplyTo(serverOptions);

                    // Applied last, so a delegate overrides what configuration set for this server while
                    // leaving untouched settings at their configured value.
                    options.ApplyServerConfigurators(server.Name, serverOptions);
                });
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

            var configuredServerNames = GetConfigurationState(context).Servers
                .Select(server => server.Name)
                .ToArray();
            foreach (var serverName in GetOptions(context).GetConfiguredServerNamesNotIn(configuredServerNames))
            {
                errors.Add(new BootstrapValidationError(
                    $"{ConfigurationSectionKey}:Servers:{serverName}",
                    "No server with this name is configured, so the options supplied for it would be discarded."));
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

            return new HangfireConfigurationState(ReadServers(section, errors), errors);
        });
    }

    /// <summary>
    /// Reads one server per entry of the <c>Servers</c> dictionary, keyed by the server name.
    /// </summary>
    /// <remarks>
    /// A dictionary rather than an array, because the key names the server and because configuration layering
    /// overrides by key: an entry keeps its identity when a later source reorders or adds servers.
    /// </remarks>
    private static HangfireServerConfiguration[] ReadServers(IConfigurationSection section,
        List<BootstrapValidationError> errors)
    {
        return section.GetSection("Servers").GetChildren()
            .Select(serverSection => new HangfireServerConfiguration(
                serverSection.Key,
                ReadQueues(serverSection, errors),
                ReadWorkerCount(serverSection, errors),
                ReadShutdownTimeout(serverSection, errors)))
            .ToArray();
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
        IReadOnlyCollection<HangfireServerConfiguration> servers,
        IReadOnlyCollection<BootstrapValidationError> errors)
    {
        public IReadOnlyCollection<HangfireServerConfiguration> Servers { get; } = servers;
        public IReadOnlyCollection<BootstrapValidationError> Errors { get; } = errors;
    }

    private sealed class HangfireServerConfiguration(
        string name,
        string[]? queues,
        int? workerCount,
        TimeSpan? shutdownTimeout)
    {
        public string Name { get; } = name;

        /// <summary>
        /// Applies the configured values, leaving the Hangfire default in place for every absent key.
        /// </summary>
        public void ApplyTo(BackgroundJobServerOptions serverOptions)
        {
            // The dictionary key names the server, so the dashboard lists it under a name the operator chose.
            serverOptions.ServerName = Name;

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
