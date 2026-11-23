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
        public required ITypeSymbol GenericQuerySymbol { get; set; }
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

    public class GenerationData
    {
        public required ResourceControllerData[] Controllers { get; set; }
        public required Diagnostic[] Diagnostics { get; set; }
    }

    public class ResourceControllerData
    {
        public required string AssemblyNamespace { get; set; }
        public required ResourceData[] ResourcesData { get; set; }
        public string BaseRoute => ResourcesData[0].MainResource.Pluralize().Kebaberize();
        public string ClassName => $"{ResourcesData[0].GroupPrefix}{ResourcesData[0].MainResource.Pascalize()}ResourceController";
        public string Namespace => $"{AssemblyNamespace}.Resources.Controllers";
        public string ClassFileName => $"{ClassName}.g.cs";
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
        public string? Group { get; set; }
        public string GroupPrefix => string.IsNullOrWhiteSpace(Group) ? string.Empty : Group!.Pascalize();
        public Parameter[] InvocationParameters { get; set; } = [];

        public Parameter[] InvocationParametersWithToken =>
            InvocationParameters.Any(parameter => parameter.Type == "System.Threading.CancellationToken")
                ? InvocationParameters
                : InvocationParameters.Concat([new Parameter {Name = "cancellationToken", PropertyName = "CancellationToken", Type = "System.Threading.CancellationToken"}]).ToArray();

        public Parameter[] NonRouteInvocationParameters => 
            InvocationParameters
                .Where(param => param.Name != "cancellationToken").ToArray()
                .Where(parameter => !KeysByResource.Values.Any(keys => keys.Contains(parameter.Name)))
                .ToArray();

        /// <summary>
        /// The name of the controller parameter that reads the body of a command, which holds the data of the command
        /// that is not a key. It is declared nullable so that MVC does not report it as required when the body cannot be
        /// read, on top of the errors of the body; the action never runs with it null, because those errors stop it.
        /// </summary>
        public const string CommandBodyParameterName = "commandBody";

        /// <summary>
        /// Whether the data of the command that is not a key is read from the body of the request.
        /// </summary>
        public bool HasCommandBody =>
            DataType == "Command"
            && ResourceType is not (ResourceType.List or ResourceType.Retrieve)
            && NonRouteInvocationParameters.Length > 0;

        public Parameter[] RouteParameters =>
            InvocationParameters
                .Where(parameter => KeysByResource.Values.Any(keys => keys.Contains(parameter.Name)))
                .ToArray();

        public Parameter[] ControllerMethodParameters =>
            HasCommandBody
                ? RouteParameters
                    .Append(new Parameter
                    {
                        Name = CommandBodyParameterName,
                        PropertyName = CommandBodyParameterName,
                        Type = FullClassName,
                    })
                    .Append(new Parameter {Name = "cancellationToken", PropertyName = "CancellationToken", Type = "System.Threading.CancellationToken"})
                    .ToArray()
                : InvocationParametersWithToken;

        /// <summary>
        /// The parameters of the controller action that read data of the command from the route or the query string,
        /// by the name of the property of the command they set.
        /// </summary>
        private Parameter[] RequestParameters => HasCommandBody ? RouteParameters : InvocationParameters;

        public string RequestParametersByPropertyString =>
            $"new Dictionary<string, string> {{ {string.Join(", ", RequestParameters.Select(parameter => $"[\"{parameter.PropertyName}\"] = \"{parameter.Name}\""))} }}";

        public string CommandPropertiesString => string.Join(",\n", InvocationParameters
            .Select(parameter => HasCommandBody && NonRouteInvocationParameters.Contains(parameter)
                ? $"{parameter.PropertyName} = {CommandBodyParameterName}!.{parameter.PropertyName}"
                : $"{parameter.PropertyName} = {parameter.Name}"));

        public string QueryPropertiesString => string.Join(",\n", InvocationParameters
            .Select(parameter => $"{parameter.PropertyName} = {parameter.Name}"));

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

        public string SourceResourceAttributeString => DataType switch
        {
            "Command" => $"[SourceCommand<{FullClassName}>]",
            "Query" => $"[SourceQuery(typeof({FullClassName}))]",
            _ => throw new ArgumentOutOfRangeException()
        };

        public string ControllerMethodParametersString =>
            string.Join(", ", ControllerMethodParameters.Select(parameter => ResourceType switch
            {
                _ when parameter.Type == "System.Threading.CancellationToken"
                    => $"{parameter.Type} {parameter.Name}",
                _ when HasCommandBody && parameter.Name == CommandBodyParameterName
                    => $"[FromCommandBody({string.Join(", ", RouteParameters.Select(key => $"\"{key.PropertyName}\""))})] {parameter.Type}? {parameter.Name}",
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

        public string HttpAttribute => string.IsNullOrEmpty(HttpRoute) ? $"[Http{HttpVerb}]" : $"[Http{HttpVerb}(\"{HttpRoute}\")]";
        
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
        /// <summary>
        /// The name of the parameter of the controller action.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The name of the property of the command or query the parameter sets.
        /// </summary>
        public required string PropertyName { get; set; }

        public required string Type { get; set; }
    }

    private static readonly string ControllerClassTemplate =
        """
        // This file was auto-generated by a source generator
        #nullable enable
        
        using System;
        using System.Collections.Generic;
        using System.Threading;
        using System.Threading.Tasks;
        using IOKode.OpinionatedFramework;
        using IOKode.OpinionatedFramework.AspNetCoreIntegrations;
        using IOKode.OpinionatedFramework.Resources.Exceptions;
        using IOKode.OpinionatedFramework.Commands;
        using IOKode.OpinionatedFramework.Commands.Extensions;
        using IOKode.OpinionatedFramework.Facades;
        using IOKode.OpinionatedFramework.Persistence.Queries;
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
            {{ resource.SourceResourceAttributeString }}
            {{~ if resource.ReturnResponseType == 'Task' ~}}
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            {{~ else ~}}
            [ProducesResponseType<{{ resource.ReturnResponseType }}>(StatusCodes.Status200OK)]
            {{~ end ~}}
            public async Task<IActionResult> {{ resource.ControllerMethodName }}({{ resource.ControllerMethodParametersString }})
            {
                {{~ if resource.DataType == 'Query' ~}}
                var query = new {{ resource.FullClassName }}
                {
                    {{ resource.QueryPropertiesString }}
                };
                var result = await query.InvokeAsync(cancellationToken);
                return Ok(result);
                {{~ else if resource.DataType == 'Command' ~}}
                var command = new {{ resource.FullClassName }}
                {
                    {{ resource.CommandPropertiesString }}
                };
                try
                {
                    {{~ if resource.GenericArgument ~}}
                    var result = await command.InvokeAsync(cancellationToken);
                    return Ok(result);
                    {{~ else ~}}
                    await command.InvokeAsync(cancellationToken);
                    return NoContent();
                    {{~ end ~}}
                }
                catch (InvalidInputException exception) when (ReferenceEquals(exception.Command, command))
                {
                    return this.InvalidInput(exception, {{ resource.RequestParametersByPropertyString }});
                }
                {{~ end ~}}
            }
            {{~ end ~}}
        }
        """;
}
