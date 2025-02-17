using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Docker.DotNet;
using Docker.DotNet.Models;
using IOKode.OpinionatedFramework.Utilities;
using Npgsql;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Helpers.Containers;

public static class PostgresHelper
{
    public static async Task WaitUntilPostgresServerIsReady(DockerClient docker, string postgresContainerId, string postgresConnectionString, ITestOutputHelper? output = null)
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
                await using var client = new NpgsqlConnection(postgresConnectionString);
                await client.OpenAsync();
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

    public static async Task<string> RunPostgresContainer(DockerClient docker, ITestOutputHelper? output = null)
    {
        var container = await docker.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Image = "postgres",
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {"5432/tcp", [new PortBinding {HostPort = "5432"}]},
                }
            },
            Env =
            [
                "POSTGRES_PASSWORD=secret",
                "POSTGRES_USER=iokode",
                "POSTGRES_DB=testdb"
            ],
            Name = "oftest_nhibernate_postgres"
        });

        var postgresContainerId = container.ID;
        await docker.Containers.StartContainerAsync(postgresContainerId, new ContainerStartParameters());
        return postgresContainerId;
    }

    public static async Task PullPostgresImage(DockerClient docker, ITestOutputHelper? output = null)
    {
        await docker.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = "postgres",
            Tag = "latest"
        }, null, new Progress<JSONMessage>(message => { output?.WriteLine(message.Status); }));
    }

    public static readonly string DefaultConnectionString = "Server=localhost; Database=testdb; User Id=iokode; Password=secret; Timeout=60; Connection Lifetime=90;";

    private static string? connectionString;
    public static string ConnectionString => connectionString ??= GetEnvConnectionString() ?? DefaultConnectionString;

    public static string? GetEnvConnectionString() => Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
}