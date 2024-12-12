using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Docker.DotNet;
using Docker.DotNet.Models;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate;
using IOKode.OpinionatedFramework.Persistence.QueryBuilder.Exceptions;
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
    private NpgsqlConnection _npgsqlClient = null!;
    private global::NHibernate.Cfg.Configuration _configuration = null!;
    private readonly ITestOutputHelper _output;

    public NHibernateTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
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

    }

    public async Task DisposeAsync()
    {
        await _npgsqlClient.CloseAsync();
        await _docker.Containers.StopContainerAsync(_containerId, new ContainerStopParameters());
        await _docker.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters());
        _docker.Dispose();
    }

    [Fact]
    public async Task EntitySet_Single_Success()
    {
        // Arrange
        var sessionFactory = _configuration.BuildSessionFactory();
        IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await _npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'testing@example.com', true);");

        // Act
        var repo = unitOfWork.GetRepository<UserRepository>();
        var user = await repo.GetByUsername("Ivan", default);

        // Assert
        Assert.Equal("Ivan", user.Username);
        Assert.Equal("testing@example.com", user.EmailAddress);
        Assert.True(user.IsActive);
        
        await _npgsqlClient.ExecuteAsync("DROP TABLE Users;");

        // user.IsActive = false;
        // await repo.AddAsync(new User
        // {
        //     Username = "Ivan",
        //     EmailAddress = "testing@example.com",
        //     IsActive = true
        // }, default);
        // await unitOfWork.SaveChangesAsync(default);
    }

    [Fact]
    public async Task EntitySet_Single_Fail()
    {
        // Arrange
        var sessionFactory = _configuration.BuildSessionFactory();
        IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await _npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'testing@example.com', true);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Ivan', 'testing@example.com', true);");
        var repo = unitOfWork.GetRepository<UserRepository>();

        // Act and Assert
        await Assert.ThrowsAsync<NonUniqueResultException>(async () =>
        {
            await repo.GetByUsername("Ivan", default);
        });
        await Assert.ThrowsAsync<EmptyResultException>(async () =>
        {
            await repo.GetByUsername("Marta", default);
        });
        
        await _npgsqlClient.ExecuteAsync("DROP TABLE Users;");
    }
    
    [Fact]
    public async Task EntitySet_SingleOrDefault_Success()
    {
        // Arrange
        var sessionFactory = _configuration.BuildSessionFactory();
        IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await _npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'testing@example.com', true);");

        // Act
        var repo = unitOfWork.GetRepository<UserRepository>();
        var user = await repo.GetByEmailAddress("testing@example.com", default);
        var userNull = await repo.GetByEmailAddress("testing@example.net", default);

        // Assert
        Assert.Equal("Ivan", user.Username);
        Assert.Equal("testing@example.com", user.EmailAddress);
        Assert.True(user.IsActive);
        Assert.Null(userNull);
        
        await _npgsqlClient.ExecuteAsync("DROP TABLE Users;");
    }
    
    [Fact]
    public async Task EntitySet_SingleOrDefault_Fail()
    {
        // Arrange
        var sessionFactory = _configuration.BuildSessionFactory();
        IUnitOfWork unitOfWork = new UnitOfWork(sessionFactory);

        await _npgsqlClient.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (id TEXT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(100) NOT NULL, is_active BOOLEAN NOT NULL);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Ivan', 'testing@example.com', true);");
        await _npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('2', 'Ivan', 'testing@example.com', true);");
        var repo = unitOfWork.GetRepository<UserRepository>();

        // Act and Assert
        var user = await repo.GetByEmailAddress("testing@example.net", default);
        Assert.Null(user);

        await Assert.ThrowsAsync<NonUniqueResultException>(async () =>
        {
            await repo.GetByEmailAddress("testing@example.com", default);
        });

        await _npgsqlClient.ExecuteAsync("DROP TABLE Users;");
    }
    
    [Fact]
    public async Task EntitySet_First_Success()
    {
        
    }
    
    [Fact]
    public async Task EntitySet_First_Fail()
    {
        
    }
    
    [Fact]
    public async Task EntitySet_FirstOrDefault_Success()
    {
        
    }
    
    [Fact]
    public async Task EntitySet_FirstOrDefault_Fail()
    {
        
    }
    
    [Fact]
    public async Task EntitySet_Many_Success()
    {
        
    }
}