using System;
using System.Linq;
using System.Reflection;
using IOKode.OpinionatedFramework.Persistence.Queries;

namespace IOKode.OpinionatedFramework.Utilities;

/// <summary>
/// Provides utility methods for reading and writing the parameters object of a query execution.
/// </summary>
/// <remarks>
/// A parameter is a public instance property of the parameters object. It is found by its property name
/// (<c>SomeId</c>) or by the name it is declared with in the SQL file (<c>some_id</c>), ignoring case.
/// </remarks>
public static class QueryParameterUtilities
{
    /// <summary>
    /// Whether the parameters object has a parameter with the given name.
    /// </summary>
    public static bool HasParameter(object? parameters, string name)
    {
        return FindParameter(parameters, name) != null;
    }

    /// <summary>
    /// Sets the value of a parameter of the parameters object.
    /// </summary>
    /// <exception cref="ArgumentException">The parameters object has no parameter with that name, or the value is not of its type.</exception>
    /// <exception cref="InvalidOperationException">The parameter cannot be written, for example because the parameters object is an anonymous object.</exception>
    public static void SetParameter(object? parameters, string name, object? value)
    {
        var property = FindParameter(parameters, name)
                       ?? throw new ArgumentException($"The query has no parameter named '{name}'.", nameof(name));

        if (property.SetMethod is not { IsPublic: true })
        {
            throw new InvalidOperationException($"The query parameter '{name}' cannot be written.");
        }

        var type = property.PropertyType;
        var isNullable = !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        var isAssignable = value == null
            ? isNullable
            : type.IsInstanceOfType(value) || Nullable.GetUnderlyingType(type)?.IsInstanceOfType(value) == true;
        if (!isAssignable)
        {
            throw new ArgumentException($"The value given to the query parameter '{name}' is not of type '{type}'.", nameof(value));
        }

        property.SetValue(parameters, value);
    }

    /// <summary>
    /// Throws when a parameter marked with <see cref="SetByMiddlewareAttribute"/> has no value.
    /// </summary>
    /// <exception cref="QueryParameterNotSetException">A parameter set by a middleware has no value.</exception>
    public static void EnsureMiddlewareParametersAreSet(object? parameters)
    {
        if (parameters == null)
        {
            return;
        }

        var unsetParameter = GetParameters(parameters)
            .FirstOrDefault(property => property.IsDefined(typeof(SetByMiddlewareAttribute)) && property.GetValue(parameters) == null);
        if (unsetParameter != null)
        {
            throw new QueryParameterNotSetException(unsetParameter.Name);
        }
    }

    private static PropertyInfo? FindParameter(object? parameters, string name)
    {
        if (parameters == null)
        {
            return null;
        }

        var normalizedName = name.Replace("_", string.Empty);
        return GetParameters(parameters).FirstOrDefault(property =>
            string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)
            || string.Equals(property.Name, normalizedName, StringComparison.OrdinalIgnoreCase));
    }

    private static PropertyInfo[] GetParameters(object parameters)
    {
        return parameters.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetIndexParameters().Length == 0)
            .ToArray();
    }
}
