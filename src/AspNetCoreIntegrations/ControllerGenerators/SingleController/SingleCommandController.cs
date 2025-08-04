using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Commands.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.ControllerGenerators.SingleController;

public class SingleCommandController : ControllerBase
{
    public async Task<IResult> InvokeCommandAsync()
    {
        var controllerStrategy = Locator.Resolve<SingleControllerStrategy>();
        if (!Request.Method.Equals(controllerStrategy.HttpMethod, StringComparison.InvariantCultureIgnoreCase))
        {
            return Results.Problem
            (
                title: "Method not allowed.",
                detail: "Invalid HTTP method.",
                statusCode: StatusCodes.Status405MethodNotAllowed
            );
        }

        var hasCommandHeader = Request.Headers.TryGetValue("X-Command", out var commandName);
        var isARegisteredCommand = CommandsMapper.Commands.TryGetValue((string?) commandName ?? string.Empty, out var commandType);
        if (!hasCommandHeader)
        {
            return Results.Problem
            (
                title: "Bad request.",
                detail: "Missing required X-Command header or invalid command type.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        if (!isARegisteredCommand)
        {
            return Results.Problem
            (
                title: "Bad request.",
                detail: "Invalid command type.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        try
        {
            var commandInstance = await CreateCommandInstance(commandType!, Request.Body);

            if (commandType!.IsAssignableTo(typeof(Command)))
            {
                return await InvokeCommandAsync((Command)commandInstance);
            }

            var executeMethod = commandInstance.GetType().GetMethod("ExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var returnType = executeMethod!.ReturnType;

            var resultType = returnType.GetGenericArguments()[0];
            var method = GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(method => method is {Name: nameof(InvokeCommandAsync), IsGenericMethodDefinition: true})?
                .MakeGenericMethod(resultType);

            return await (Task<IResult>) method!.Invoke(this, [commandInstance])!;
        }
        catch (JsonException ex)
        {
            return Results.Problem
            (
                title: "Bad request.",
                detail: "Invalid JSON format.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem
            (
                title: "Bad request.",
                detail: "Invalid command data.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            return Results.Problem
            (
                title: "Bad request.",
                detail: "Failed to create command instance.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
    }
    
    private async Task<IResult> InvokeCommandAsync(Command command)
    {
        await command.InvokeAsync();
        return Results.Ok();
    }

    private async Task<IResult> InvokeCommandAsync<T>(Command<T> command)
    {
        var commandResult = await command.InvokeAsync();
        return Results.Ok(commandResult);
    }

    private static async Task<object> CreateCommandInstance(Type commandType, Stream jsonStream)
    {
        using var reader = new StreamReader(jsonStream);
        var jsonString = await reader.ReadToEndAsync();

        var serializerOptions = new JsonSerializerOptions();
        foreach (var converterType in JsonConvertersMapper.Converters)
        {
            var converter = (JsonConverter) Activator.CreateInstance(converterType)!;
            serializerOptions.Converters.Add(converter);
        }
        var dictionary = (!string.IsNullOrWhiteSpace(jsonString)
            ? JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString, serializerOptions)
            : null) ?? new Dictionary<string, JsonElement>();

        var constructor = commandType.GetConstructors().FirstOrDefault();
        if (constructor == null)
        {
            throw new ArgumentException($"No constructor found for {commandType.FullName}.");
        }

        var parameters = constructor.GetParameters();
        if (parameters.Length == 0)
        {
            return constructor.Invoke([]);
        }

        var values = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var paramInfo = parameters[i];
            var paramName = paramInfo.Name;
            if (paramName == null)
            {
                throw new ArgumentException($"No parameter name found in {commandType.FullName}.");
            }

            if (!dictionary.TryGetValue(paramName, out var jsonElement))
            {
                throw new ArgumentException($"Missing parameter '{paramName}' in JSON.");
            }

            values[i] = JsonSerializer.Deserialize(jsonElement.GetRawText(), paramInfo.ParameterType, serializerOptions);
        }

        return constructor.Invoke(values);
    }
}