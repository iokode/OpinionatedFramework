using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Docker.DotNet;
using Docker.DotNet.Models;
using IOKode.OpinionatedFramework.Utilities;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate;

public abstract class NHibernateTestsBase(ITestOutputHelper output) : IAsyncLifetime
{
    protected string _containerId = null!;
    protected DockerClient _docker = null!;
    protected NpgsqlConnection _npgsqlClient = null!;
    protected global::NHibernate.Cfg.Configuration _configuration = null!;

    public async Task InitializeAsync()
    {
        _docker = new DockerClientConfiguration().CreateClient();
        await pullPostgresImage();
        await runPostgresContainer();
        await waitUntilPostgresServerIsReady();

        string dbConnectionString = "Server=localhost; Database=testdb; User Id=iokode; Password=secret;";
        
        _configuration = new global::NHibernate.Cfg.Configuration();
        _configuration.Properties.Add("connection.connection_string", dbConnectionString);
        _configuration.Properties.Add("dialect", "NHibernate.Dialect.PostgreSQL83Dialect");
        _configuration.AddXmlFile("user.hbm.xml");
        
        _npgsqlClient = new NpgsqlConnection(dbConnectionString);
        await _npgsqlClient.OpenAsync();
        
        async Task waitUntilPostgresServerIsReady()
        {
            bool postgresServerIsReady = await PollingUtility.WaitUntilTrueAsync(async () =>
            {
                var containerInspect = await _docker.Containers.InspectContainerAsync(_containerId);
                bool containerIsReady = containerInspect.State.Running;
                if (!containerIsReady)
                {
                    return false;
                }

                try
                {
                    var dbConnectionString = "Server=localhost; Database=testdb; User Id=iokode; Password=secret;";
                    var client = new NpgsqlConnection(dbConnectionString);
                    await client.OpenAsync();
                    await client.QuerySingleAsync<int>("SELECT 1");
                    await client.CloseAsync();
                    
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }, timeout: 30_000, pollingInterval: 1_000);

            if (!postgresServerIsReady)
            {
                output.WriteLine("Failed to start Postgres server within the allowed time (30s).");
            }
        }

        async Task runPostgresContainer()
        {
            var container = await _docker.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Image = "postgres",
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "5432/tcp", [new PortBinding { HostPort = "5432" }] },
                    }
                },
                Env = [
                    "POSTGRES_PASSWORD=secret",
                    "POSTGRES_USER=iokode",
                    "POSTGRES_DB=testdb"
                ],
                Name = "oftest_nhibernate_postgres"
            });

            _containerId = container.ID;
            await _docker.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        }
        
        async Task pullPostgresImage()
        {
            await _docker.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = "postgres",
                Tag = "latest"
            }, null, new Progress<JSONMessage>(message => { output.WriteLine(message.Status); }));
        }

    }

    public async Task DisposeAsync()
    {
        await _npgsqlClient.CloseAsync();
        await _docker.Containers.StopContainerAsync(_containerId, new ContainerStopParameters());
        await _docker.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters());
        _docker.Dispose();
    }
}