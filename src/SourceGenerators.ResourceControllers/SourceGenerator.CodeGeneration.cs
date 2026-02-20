using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using Microsoft.CodeAnalysis;

namespace IOKode.OpinionatedFramework.SourceGenerators.ResourceControllers;

public partial class SourceGenerator
{
    public enum ResourceType
    {
        Retrieve,
        List,
        Create,
        Update,
        Replace,
        Delete,
        Action
    }

    public class GeneratorContextData
    {
        public required ITypeSymbol CommandBaseSymbol { get; set; }
        public required ITypeSymbol GenericCommandBaseSymbol { get; set; }
        public required IAssemblySymbol CompilationAssemblySymbol { get; set; }
        public required string AssemblyNamespace { get; set; }

        public readonly IReadOnlyDictionary<string, ResourceType> ResourceTypes = new Dictionary<string, ResourceType>
        {
            ["IOKode.OpinionatedFramework.Resources.Attributes.RetrieveResourceAttribute"] = ResourceType.Retrieve,
            ["IOKode.OpinionatedFramework.Resources.Attributes.ListResourcesAttribute"] = ResourceType.List,
            ["IOKode.OpinionatedFramework.Resources.Attributes.CreateResourceAttribute"] = ResourceType.Create,
            ["IOKode.OpinionatedFramework.Resources.Attributes.UpdateResourceAttribute"] = ResourceType.Update,
            ["IOKode.OpinionatedFramework.Resources.Attributes.ReplaceResourceAttribute"] = ResourceType.Replace,
            ["IOKode.OpinionatedFramework.Resources.Attributes.DeleteResourceAttribute"] = ResourceType.Delete,
            ["IOKode.OpinionatedFramework.Resources.Attributes.ActionOnResourceAttribute"] = ResourceType.Action,
        };
    }

    public class ResourceControllerData
    {
        public required string AssemblyNamespace { get; set; }
        public required ResourceData[] ResourcesData { get; set; }
        public string BaseRoute => ResourcesData[0].MainResource.Pluralize().Kebaberize();
        public string ClassName => $"{ResourcesData[0].MainResource.Pascalize()}ResourceController";
        public string Namespace => $"{AssemblyNamespace}.Resources.Controllers";
        public string ClassFileName => $"{ClassName}.g.cs";
        public ResourceData[] ResourcesDataWithBodyClass => ResourcesData.Where(resource => resource.BodyInputClassName != null).ToArray();
    }

    public abstract class ResourceData
    {
        public abstract string ReturnResponseType { get; }
        public abstract string DataType { get; } 
        public required string ResourceValue { get; set; }
        public required string ClassName { get; set; }
        public required string Namespace { get; set; }
        public required ResourceType ResourceType { get; set; }
        public string? DocComment { get; set; }
        public string? KeyValue { get; set; }
        public string? Action { get; set; }
        public Parameter[] InvocationParameters { get; set; } = [];

        public Parameter[] InvocationParametersWithToken =>
            InvocationParameters.Any(parameter => parameter.Type == "System.Threading.CancellationToken")
                ? InvocationParameters
                : InvocationParameters.Concat([new Parameter {Name = "cancellationToken", Type = "System.Threading.CancellationToken"}]).ToArray();

        public Parameter[] NonRouteInvocationParameters => 
            InvocationParameters
                .Where(param => param.Name != "cancellationToken").ToArray()
                .Where(parameter => !KeysByResource.Values.Any(keys => keys.Contains(parameter.Name)))
                .ToArray();

        public string? BodyInputClassName =>
            NonRouteInvocationParameters.Length > 1 && ResourceType is not (ResourceType.List or ResourceType.Retrieve)
                ? MainResource.Pascalize() + ControllerMethodName.Replace("Async", "Input")
                : null;

        public Parameter[] ControllerMethodParameters =>
            BodyInputClassName == null
                ? InvocationParametersWithToken
                : InvocationParametersWithToken
                    .Select(parameter => !NonRouteInvocationParameters.Contains(parameter)
                        ? parameter
                        : parameter == NonRouteInvocationParameters.First()
                            ? new Parameter
                            {
                                Type = BodyInputClassName,
                                Name = "input",
                            }
                            : null)
                    .Where(parameter => parameter != null)
                    .Cast<Parameter>()
                    .ToArray();

        public string CommandParametersString => string.Join(", ", InvocationParameters
            .Select(parameter => BodyInputClassName != null && NonRouteInvocationParameters.Contains(parameter)
                ? $"input.{parameter.PascalizedName}"
                : parameter.Name));

        public string FullClassName => $"{Namespace}.{ClassName}";
        public string[] Resources => ResourceValue.Split('/').Select(resource => resource.Trim()).ToArray();
        public string MainResource => Resources.FirstOrDefault() ?? string.Empty;
        public Dictionary<string, string[]> KeysByResource
        {
            get
            {
                var keysSplitByResource = KeyValue?.Split('/').Select(key => key.Trim()).ToArray() ?? [];
                return Resources.Select((resource, idx) =>
                (
                    resource,
                    keys: idx < keysSplitByResource.Length
                        ? keysSplitByResource[idx].Split(',').Select(key => key.Trim().Camelize()).ToArray()
                        : []
                )).Select(tuple => tuple.keys.Length == 1 && string.IsNullOrEmpty(tuple.keys[0])
                    ? tuple with {keys = []}
                    : tuple
                ).ToDictionary(tuple => tuple.resource, x => x.keys);
            }
        }

        public string ControllerMethodParametersString =>
            string.Join(", ", ControllerMethodParameters.Select(parameter => ResourceType switch
            {
                _ when parameter.Type == "System.Threading.CancellationToken"
                    => $"{parameter.Type} {parameter.Name}",
                _ when KeysByResource.Values.Any(keys => keys.Contains(parameter.Name, StringComparer.InvariantCultureIgnoreCase))
                    => $"[FromRoute] {parameter.Type} {parameter.Name}",
                ResourceType.Retrieve or
                    ResourceType.List 
                    => $"[FromQuery] {parameter.Type} {parameter.Name}",
                ResourceType.Create or
                    ResourceType.Update or
                    ResourceType.Replace or
                    ResourceType.Delete or
                    ResourceType.Action
                    => $"[FromBody] {parameter.Type} {parameter.Name}",
                _ => throw new ArgumentOutOfRangeException()
            }));

        public string HttpVerb => ResourceType switch
        {
            ResourceType.Retrieve => "Get",
            ResourceType.List => "Get",
            ResourceType.Create => "Post",
            ResourceType.Update => "Patch",
            ResourceType.Replace => "Put",
            ResourceType.Delete => "Delete",
            ResourceType.Action => "Patch",
            _ => throw new ArgumentOutOfRangeException()
        };

        public string HttpAttribute => ResourceType switch
        {
            ResourceType.Create => $"[Http{HttpVerb}]",
            _ => string.IsNullOrEmpty(HttpRoute) ? $"[Http{HttpVerb}]" : $"[Http{HttpVerb}(\"{HttpRoute}\")]"
        };
        
        public string HttpRoute
        {
            get
            {
                var builder = new StringBuilder();

                foreach (var resource in Resources)
                {
                    if (resource != MainResource)
                    {
                        builder.Append($"{resource.Kebaberize()}/");
                    }

                    var notEmptyKeys = KeysByResource[resource].Where(str => !string.IsNullOrEmpty(str));
                    foreach (var key in notEmptyKeys)
                    {
                        builder.Append($"{key.Kebaberize()}-{{{key}}}/");
                    }
                }

                if (Action != null)
                {
                    builder.Append(Action.Kebaberize());
                }

                return builder.ToString();
            }
        }

        public string ControllerMethodName
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append(ResourceType.ToString().Pascalize());

                var subresources = Resources.Skip(1);
                foreach (var resource in subresources)
                {
                    builder.Append(resource.Pascalize());
                }

                if (Action != null)
                {
                    builder.Append(Action.Pascalize());
                }

                if (KeysByResource.Values.Any(values => values.Any()))
                {
                    builder.Append("By");

                    var resourceKeys = KeysByResource
                        .Where(keyValue => keyValue.Value.Any())
                        .SelectMany(keyValue => keyValue.Key == Resources.Last()
                            ? keyValue.Value.Select(value => value.Pascalize())
                            : keyValue.Value.Select(value => $"{keyValue.Key.Pascalize()}{value.Pascalize()}"));
                    var resourceKeysString = string.Join("And", resourceKeys);
                    builder.Append(resourceKeysString);
                }

                builder.Append("Async");
                return builder.ToString();
            }
        }
    }

    public class QueryData : ResourceData
    {
        public required string MethodReturnType { get; set; }
        public override string ReturnResponseType
        {
            get
            {
                var match = Regex.Match(MethodReturnType, "Task<(.+)>");
                return match.Success ? match.Groups[1].Value : "Task";
            }
        }
        public override string DataType => "Query";
    }

    public class CommandData : ResourceData
    {
        public string? GenericArgument { get; set; }
        public override string ReturnResponseType => GenericArgument ?? "Task";
        public override string DataType => "Command";
    }

    public class Parameter
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public string PascalizedName => Name.Pascalize();
    }

    private static readonly string ControllerClassTemplate =
        """
        // This file was auto-generated by a source generator
        #nullable enable
        
        using System;
        using System.Threading;
        using System.Threading.Tasks;
        using IOKode.OpinionatedFramework;
        using IOKode.OpinionatedFramework.AspNetCoreIntegrations;
        using IOKode.OpinionatedFramework.AspNetCoreIntegrations.Exceptions;
        using IOKode.OpinionatedFramework.Commands;
        using IOKode.OpinionatedFramework.Commands.Extensions;
        using IOKode.OpinionatedFramework.Facades;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Mvc;

        namespace {{ Namespace }};

        [ApiController]
        [Route("{{ BaseRoute }}")]
        public partial class {{ ClassName }} : Controller
        {
            {{~ for resource in ResourcesData ~}}
            {{~ if resource.DocComment ~}}
            {{ resource.DocComment }}
            {{~ end ~}}
            {{ resource.HttpAttribute }}
            [SourceCommand("{{ resource.FullClassName }}")]
            {{~ if resource.ReturnResponseType == 'Task' ~}}
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            {{~ else ~}}
            [ProducesResponseType<{{ resource.ReturnResponseType }}>(StatusCodes.Status200OK)]
            {{~ end ~}}
            [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
            [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> {{ resource.ControllerMethodName }}({{ resource.ControllerMethodParametersString }})
            {
                try
                {
                    {{~ if resource.DataType == 'Query' ~}}
                    var result = await {{ resource.FullClassName }}.InvokeAsync({{ for parameter in resource.InvocationParameters }}{{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }});
                    return Ok(result);
                    {{~ else if resource.DataType == 'Command' ~}}
                    var command = new {{ resource.FullClassName }}({{ resource.CommandParametersString }});
                    {{~ if resource.GenericArgument ~}}
                    var result = await command.InvokeAsync(cancellationToken);
                    return Ok(result);
                    {{~ else ~}}
                    await command.InvokeAsync(cancellationToken);
                    return NoContent();
                    {{~ end ~}}
                    {{~ end ~}}
                }
                catch (ResourceNotFoundException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An unexpected error occurred.");
                    return Problem
                    (
                        title: "Internal Server Error",
                        detail: "An unexpected error occurred. Please contact support if the problem persists.",
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }
            }
            {{~ end ~}}
        }

        {{~ for resource in ResourcesDataWithBodyClass ~}}
        public record {{ resource.BodyInputClassName }}
        {
            {{~ for parameter in resource.NonRouteInvocationParameters ~}}
            public required {{ parameter.Type }} {{ parameter.PascalizedName }} { get; init; }
            {{~ end ~}}
        }
        {{~ end ~}}
        """;
}