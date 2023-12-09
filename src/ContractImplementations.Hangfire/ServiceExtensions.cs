using System;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;
using IOKode.OpinionatedFramework.Jobs;
using JsonNet.ContractResolvers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

public static class ServiceExtensions
{
    // todo remove after test
    public static void AddHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IJobEnqueuer, HangfireJobEnqueuer>();
        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();

        var dbOptions = new MongoOptions();
        configuration.GetSection("Hangfire:Mongo").Bind(dbOptions);

        var storageOptions = new MongoStorageOptions
        {
            MigrationOptions = new MongoMigrationOptions
            {
                MigrationStrategy = new DropMongoMigrationStrategy(),
                BackupStrategy = new CollectionMongoBackupStrategy()
            },
            CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
        };

        services.AddHangfire(configuration =>
        {
            configuration
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(dbOptions.ConnectionString, dbOptions.Database, storageOptions);

            configuration.UseSerializerSettings(new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new PrivateSetterContractResolver(), // todo review
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
                // SerializationBinder = new SerializationBinder(),
                // Converters = 
            });

            // configuration.UseTypeResolver(Plugin.GetType);
            configuration.UseTypeSerializer(type => type.AssemblyQualifiedName);
        });

        services.AddHangfireServer(option =>
        {
            option.SchedulePollingInterval = TimeSpan.FromSeconds(1);
            option.CancellationCheckInterval = TimeSpan.FromSeconds(1);
        });
    }
}