using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Configuration.Exceptions;
using IOKode.OpinionatedFramework.Ensuring;
using Microsoft.Extensions.Configuration;
using IConfigurationProvider = IOKode.OpinionatedFramework.Configuration.IConfigurationProvider;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftConfiguration;

public class MicrosoftConfigurationProvider : IConfigurationProvider
{
    private readonly IConfiguration configuration;

    public MicrosoftConfigurationProvider(IConfiguration configuration)
    {
        Ensure.ArgumentNotNull(configuration);

        this.configuration = configuration;
    }

    public T? GetValue<T>(string key)
    {
        try
        {
            var value = this.configuration.GetValue<T>(key);

            // If value is default(T), it means either key is not present or it's a complex object that needs binding
            if (EqualityComparer<T>.Default.Equals(value, default))
            {
                var section = this.configuration.GetSection(key);
                if (section.Exists())
                {
                    var result = Activator.CreateInstance<T>();
                    section.Bind(result);
                    return result;
                }
            }

            return value;
        }
        catch (InvalidOperationException ex)
        {
            throw new TypeMismatchException(typeof(T), null, ex);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException)
        {
            throw new TypeMismatchException(typeof(T), null, ex);
        }
    }
}