using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

/// <summary>
/// Reads a command from the JSON body of the request with the serializer options of MVC, leaving out the properties
/// that are keys of the resource, as <see cref="FromCommandBodyAttribute"/> says. A value that cannot be read is
/// reported as a model state error with the path of its field. MVC does not validate the command it reads: the command
/// validates its own data when it is invoked.
/// </summary>
public sealed class CommandBodyModelBinder : IModelBinder
{
    private static readonly ConcurrentDictionary<(JsonSerializerOptions, Type, string), JsonSerializerOptions> bodyOptionsCache = new();

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var attribute = GetAttribute(bindingContext.ModelMetadata);
        var mvcOptions = bindingContext.HttpContext.RequestServices.GetRequiredService<IOptions<MvcJsonOptions>>().Value.JsonSerializerOptions;
        var bodyOptions = GetBodyOptions(mvcOptions, bindingContext.ModelType, attribute.KeyProperties);

        object? model;
        try
        {
            model = await JsonSerializer.DeserializeAsync(
                bindingContext.HttpContext.Request.Body,
                bindingContext.ModelType,
                bodyOptions,
                bindingContext.HttpContext.RequestAborted);
        }
        catch (JsonException exception)
        {
            bindingContext.ModelState.AddModelError(exception.Path ?? "$", exception.Message);
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        if (model is null)
        {
            bindingContext.ModelState.AddModelError("$", "The body of the request cannot be null.");
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        // The data of a command is validated by the command itself when it is invoked, not by MVC.
        bindingContext.ValidationState[model] = new ValidationStateEntry { SuppressValidation = true };
        bindingContext.Result = ModelBindingResult.Success(model);
    }

    private static FromCommandBodyAttribute GetAttribute(ModelMetadata metadata)
    {
        var attributes = (metadata as DefaultModelMetadata)?.Attributes.ParameterAttributes;
        return attributes?.OfType<FromCommandBodyAttribute>().SingleOrDefault()
               ?? throw new InvalidOperationException(
                   $"{nameof(CommandBodyModelBinder)} binds only a parameter marked with {nameof(FromCommandBodyAttribute)}.");
    }

    /// <summary>
    /// The serializer options of MVC, with the key properties of <paramref name="commandType"/> left out of its JSON
    /// contract, so that the body neither has to nor can carry them.
    /// </summary>
    private static JsonSerializerOptions GetBodyOptions(JsonSerializerOptions mvcOptions, Type commandType,
        IReadOnlyCollection<string> keyProperties)
    {
        var cacheKey = (mvcOptions, commandType, string.Join(",", keyProperties.Order(StringComparer.Ordinal)));
        return bodyOptionsCache.GetOrAdd(cacheKey, _ =>
        {
            var resolver = mvcOptions.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
            return new JsonSerializerOptions(mvcOptions)
            {
                TypeInfoResolver = resolver.WithAddedModifier(typeInfo =>
                {
                    if (typeInfo.Type != commandType || typeInfo.Kind != JsonTypeInfoKind.Object)
                    {
                        return;
                    }

                    var keys = typeInfo.Properties
                        .Where(property => property.AttributeProvider is PropertyInfo clrProperty && keyProperties.Contains(clrProperty.Name))
                        .ToArray();
                    if (keys.Length != keyProperties.Count)
                    {
                        throw new InvalidOperationException(
                            $"The keys '{string.Join("', '", keyProperties)}' of the command '{commandType}' are not all properties of its JSON contract.");
                    }

                    foreach (var key in keys)
                    {
                        typeInfo.Properties.Remove(key);
                    }
                })
            };
        });
    }
}
