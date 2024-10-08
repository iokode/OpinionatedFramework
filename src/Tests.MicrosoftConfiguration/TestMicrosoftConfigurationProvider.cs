﻿using System.Collections.Generic;
using IOKode.OpinionatedFramework.Configuration.Exceptions;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftConfiguration;
using Microsoft.Extensions.Configuration;
using Xunit;
using IConfigurationProvider = IOKode.OpinionatedFramework.Configuration.IConfigurationProvider;

namespace IOKode.OpinionatedFramework.Tests.MicrosoftConfiguration;

public class TestMicrosoftConfigurationProvider
{
    private readonly IConfigurationProvider provider;

    public TestMicrosoftConfigurationProvider()
    {
        var configDict = new Dictionary<string, string?>
        {
            { "IsInstalled", "true" },
            { "HasPermissions", "false" },
            { "AppName", "OpinionatedFramework" },
            { "InstallationVersion", "3" },
            { "InstallationProblems", "0" },
            { "User:Name", "Alice" },
            { "User:Email", "alice@example.com" },
            { "User:Permissions", null },
            { "User:Friend:Name", "Bob" },
            { "User:Friend:Email", "bob@example.com" },
            { "User:Friend:Permissions:0", "Read" },
            { "User:Friend:Permissions:1", "Write" },
            { "AllowedIds:0", "1" },
            { "AllowedIds:1", "2" },
            { "AllowedIds:2", "3" },
            { "User:Computers:0:Brand", "Dell" },
            { "User:Computers:0:Model", "Inspiron" },
            { "User:Computers:0:Price", "10.00" },
            { "User:Computers:1:Brand", "HP" },
            { "User:Computers:1:Model", "Elite" },
            { "User:Computers:1:Price", "2000" },
            { "User:Computers:2:Brand", "Lenovo" },
            { "User:Computers:2:Model", "ThinkPad" },
            { "User:Computers:2:Price", "30.45" }
        };

        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        provider = new MicrosoftConfigurationProvider(configurationRoot);
    }

    [Fact]
    public void TestGetValueString()
    {
        // Act
        string value = provider.GetValue<string>("AppName")!;

        // Assert
        Assert.Equal("OpinionatedFramework", value);
    }

    [Fact]
    public void TestGetValueInt()
    {
        // Act
        int value = provider.GetValue<int>("InstallationVersion")!;

        // Assert
        Assert.Equal(3, value);
    }

    [Fact]
    public void TestGetDefaultValue()
    {
        // Act
        int value = provider.GetValue<int>("InstallationProblems");

        // Assert
        Assert.Equal(0, value);
    }

    [Fact]
    public void TestGetIntValueOnNotInt()
    {
        // Act & Assert
        Assert.Throws<TypeMismatchException>(() => { provider.GetValue<int>("AppName"); });
    }

    [Fact]
    public void TestGetValueBool()
    {
        // Act
        bool isInstalled = provider.GetValue<bool>("IsInstalled")!;
        bool hasPermission = provider.GetValue<bool>("HasPermissions")!;

        // Assert
        Assert.True(isInstalled);
        Assert.False(hasPermission);
    }

    [Fact]
    public void TestGetValueObject()
    {
        // Act
        var value = provider.GetValue<User>("User")!;

        // Assert
        Assert.Equal("Alice", value.Name);
        Assert.Equal("alice@example.com", value.Email);
        Assert.Null(value.Permissions);

        Assert.Equal("Bob", value.Friend!.Name);
        Assert.Equal("bob@example.com", value.Friend!.Email);
        Assert.Equal(new[] { "Read", "Write" }, value.Friend!.Permissions);
        Assert.Null(value.Friend!.Friend);
    }

    [Fact]
    public void TestGetValueAsArrayOfStrings()
    {
        // Act
        var value = provider.GetValue<string[]>("User:Friend:Permissions")!;

        // Assert
        Assert.Equal(new[] { "Read", "Write" }, value);
    }

    [Fact]
    public void TestGetValueAsArrayOfInts()
    {
        // Act
        var value = provider.GetValue<int[]>("AllowedIds")!;

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, value);
    }

    [Fact]
    public void TestGetValueAsArrayOfObjects()
    {
        // Act
        var value = provider.GetValue<Computer[]>("User:Computers")!;

        // Assert
        Assert.Equal(new[]
        {
            new Computer { Brand = "Dell", Model = "Inspiron", Price = 10.00m },
            new Computer { Brand = "HP", Model = "Elite", Price = 2000m },
            new Computer { Brand = "Lenovo", Model = "ThinkPad", Price = 30.45m }
        }, value);
    }
}