using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Commands.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers.SingleController;

public class SingleCommandController : ControllerBase
{
    // [CommandTypeConstraint(IsGeneric = false)]
    // public async Task<IActionResult> InvokeCommand(Command command)
    // {
    //     await command.InvokeAsync();
    //     return Ok();
    // }
    //
    // [CommandTypeConstraint(IsGeneric = true)]
    // [ActionName("InvokeCommand")]
    // public async Task<IActionResult> InvokeGenericCommand<T>(Command<T> command)
    // {
    //     var commandResult = await command.InvokeAsync();
    //     return Ok(commandResult);
    // }

    public async Task<IResult> InvokeCommand()
    {
        var controllerStrategy = Locator.Resolve<SingleControllerStrategy>();
        if (!Request.Method.Equals(controllerStrategy.HttpMethod, StringComparison.InvariantCultureIgnoreCase))
        {
            return Results.Problem
            (
                title: "Method not allowed",
                detail: "Invalid HTTP method",
                statusCode: StatusCodes.Status405MethodNotAllowed
            );
        }

        var hasCommandHeader = Request.Headers.TryGetValue("X-Command", out var commandName);
        var isARegisteredCommand = CommandsMapper.Commands.TryGetValue((string?) commandName ?? string.Empty, out var commandType);
        if (!hasCommandHeader)
        {
            return Results.BadRequest(new {error = "Missing required X-Command header or invalid command type"});
        }

        if (!isARegisteredCommand)
        {
            return Results.BadRequest(new {error = "Invalid command type"});
        }

        try
        {
            var commandInstance = await CreateCommandInstance(commandType!, Request.Body);
            return await InvokeCommand((dynamic) commandInstance);
            // return commandInstance switch
            // {
            //     Command command => await InvokeCommand(command),
            //     var command when command.GetType().IsAssignableToOpenGeneric(typeof(Command<>)) => await InvokeCommand((dynamic) command),
            // };
        }
        catch (JsonException ex)
        {
            return Results.BadRequest(new {error = "Invalid JSON format", details = ex.Message});
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new {error = "Invalid command data", details = ex.Message});
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new {error = "Failed to create command instance", details = ex.Message});
        }
    }
    
    private async Task<IResult> InvokeCommand(Command command)
    {
        await command.InvokeAsync();
        return Results.Ok();
    }

    private async Task<IResult> InvokeCommand<T>(Command<T> command)
    {
        var commandResult = await command.InvokeAsync();
        return Results.Ok(commandResult);
    }

    private static async Task<object> CreateCommandInstance(Type commandType, Stream jsonStream)
    {
        using var reader = new StreamReader(jsonStream);
        var jsonString = await reader.ReadToEndAsync();

        var dictionary = (!string.IsNullOrWhiteSpace(jsonString)
            ? JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString)
            : null) ?? new Dictionary<string, JsonElement>();

        var constructor = commandType.GetConstructors().FirstOrDefault();
        if (constructor == null)
        {
            throw new InvalidOperationException($"No constructor found for {commandType.FullName}");
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
                throw new InvalidOperationException($"No parameter name found in {commandType.FullName}");
            }

            if (!dictionary.TryGetValue(paramName, out var jsonElement))
            {
                throw new InvalidOperationException($"Missing parameter '{paramName}' in JSON.");
            }

            values[i] = JsonSerializer.Deserialize(jsonElement.GetRawText(), paramInfo.ParameterType);
        }

        return constructor.Invoke(values);
    }
}

public class HttpMethodWithNotAllowedConstraint : IRouteConstraint
{
    private readonly string[] _allowedMethods;

    public HttpMethodWithNotAllowedConstraint(params string[] allowedMethods)
    {
        _allowedMethods = allowedMethods ?? throw new ArgumentNullException(nameof(allowedMethods));
    }

    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, 
        RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (httpContext == null || routeDirection == RouteDirection.UrlGeneration)
            return true;

        var method = httpContext.Request.Method;
        var isMethodAllowed = _allowedMethods.Contains(method, StringComparer.OrdinalIgnoreCase);

        if (!isMethodAllowed)
        {
            // Establecer código de estado y header Allow
            httpContext.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            httpContext.Response.Headers.Allow = string.Join(", ", _allowedMethods);
            
            // Opcional: escribir mensaje de error
            // WriteMethodNotAllowedResponse(httpContext, method);
            
            return false;
        }

        return true;
    }

    private static void WriteMethodNotAllowedResponse(HttpContext context, string attemptedMethod)
    {
        context.Response.ContentType = "application/json";
        
        var errorResponse = new
        {
            error = "Method Not Allowed",
            message = $"The HTTP method '{attemptedMethod}' is not allowed for this endpoint",
            allowedMethods = string.Join(", ", ((HttpMethodWithNotAllowedConstraint)context.Items["constraint"]!)._allowedMethods)
        };

        // Escribir respuesta de forma asíncrona
        _ = Task.Run(async () =>
        {
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
        });
    }
}


