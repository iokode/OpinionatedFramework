using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Docker.DotNet;
using Docker.DotNet.Models;
using IOKode.OpinionatedFramework.TestHelpers.Configuration;
using IOKode.OpinionatedFramework.Utilities;
using Npgsql;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.TestHelpers.Containers;

public static class PostgresHelper
{
    public static async Task WaitUntilPostgresServerIsReady(DockerClient docker, string postgresContainerId, PostgresOptions postgresOptions, ITestOutputHelper? output = null)
    {
        bool postgresServerIsReady = await PollingUtility.WaitUntilTrueAsync(async () =>
        {
            output?.WriteLine("Waiting for Postgres server to be ready...");
            var containerInspect = await docker.Containers.InspectContainerAsync(postgresContainerId);
            bool containerIsReady = containerInspect.State.Running;
            if (!containerIsReady)
            {
                output?.WriteLine("Not ready yet...");
                return false;
            }

            try
            {
                await using var client = await postgresOptions.OpenConnectionAsync(CancellationToken.None);
                await client.QuerySingleAsync<int>("SELECT 1");
                await client.CloseAsync();

                return true;
            }
            catch (Exception ex)
            {
                output?.WriteLine("Not ready yet...");
                output?.WriteLine(ex.Message);
                return false;
            }
        }, timeout: 60_000, pollingInterval: 1_000);

        if (!postgresServerIsReady)
        {
            throw new TimeoutException("Failed to start Postgres server within the allowed time (60s).");
        }
    }

    public static async Task<string> RunPostgresContainer(DockerClient docker, PostgresOptions options)
    {
        var container = await docker.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Image = options.ImageWithTag,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {"5432/tcp", [new PortBinding {HostPort = options.HostPort}]},
                }
            },
            Env =
            [
                "POSTGRES_PASSWORD=secret",
                "POSTGRES_USER=iokode",
                "POSTGRES_DB=testdb"
            ],
            Name = options.ContainerName
        });

        var postgresContainerId = container.ID;
        await docker.Containers.StartContainerAsync(postgresContainerId, new ContainerStartParameters());
        return postgresContainerId;
    }

    public static async Task PullPostgresImage(DockerClient docker, PostgresOptions options, ITestOutputHelper? output = null)
    {
        await docker.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = options.Image,
            Tag = options.Tag
        }, null, new Progress<JSONMessage>(message => { output?.WriteLine(message.Status); }));
    }

    public static readonly string DefaultConnectionString = "Server=localhost; Database=testdb; User Id=iokode; Password=secret; Timeout=60; Connection Lifetime=90;";
    
    public static string ConnectionString => Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") ?? DefaultConnectionString;
}
