using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Docker.DotNet;
using Docker.DotNet.Models;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using IOKode.OpinionatedFramework.Utilities;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate;

public class NHibernateTests : IAsyncLifetime
{
    private string _containerId = null!;
    private DockerClient _docker = null!;
    private readonly ITestOutputHelper _output;

    public NHibernateTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    public async Task InitializeAsync()
    {
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
                _output.WriteLine("Failed to start Postgres server within the allowed time (30s).");
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
            }, null, new Progress<JSONMessage>(message => { _output.WriteLine(message.Status); }));
        }

        _docker = new DockerClientConfiguration().CreateClient();
        await pullPostgresImage();
        await runPostgresContainer();
        await waitUntilPostgresServerIsReady();
    }

    public async Task DisposeAsync()
    {
        await _docker.Containers.StopContainerAsync(_containerId, new ContainerStopParameters());
        await _docker.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters());
        _docker.Dispose();
    }

    [Fact]
    public async Task TestAsync()
    {
        string connectionString = "Server=localhost; Database=testdb; User Id=iokode; Password=secret;";
        
        var config = new global::NHibernate.Cfg.Configuration();
        config.Properties.Add("connection.connection_string", connectionString);
        config.Properties.Add("dialect", "NHibernate.Dialect.PostgreSQL83Dialect");
        config.AddXmlFile("user.hbm.xml");
        
        var sessionFactory = config.BuildSessionFactory();
        IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        var repo = unitOfWork.GetRepository<UserRepository>();
        
        var user2 = await repo.GetByUsername("Ivan", default);
        user2.IsActive = false;

        await repo.AddAsync(new User
        {
            Username = "Ivan",
            EmailAddress = "testing@iokode.net",
            IsActive = true
        }, default);
        
        await unitOfWork.SaveChangesAsync(default);
    }
}