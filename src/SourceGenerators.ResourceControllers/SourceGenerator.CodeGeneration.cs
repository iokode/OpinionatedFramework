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
        public ResourceData[] ResourcesDataWithBody => ResourcesData.Where(resource => resource.HasBody).ToArray();
        public bool HasResourcesWithBody => ResourcesData.Any(resource => resource.HasBody);
        public ResourceData[] ResourcesDataWithQueryString => ResourcesData.Where(resource => resource.HasQueryString).ToArray();
        public bool HasResourcesWithQueryString => ResourcesData.Any(resource => resource.HasQueryString);
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
        /// The name of the controller parameter that reads the body of the request into <see cref="BodyClassName"/>.
        /// </summary>
        /// <remarks>
        /// The parameter is nullable only so that MVC does not add "The body field is required." next to the error of a
        /// body that cannot be read (https://github.com/dotnet/aspnetcore/issues/53058). Whenever it is null the model
        /// state has an error, an empty body included, so the check of the model state at the start of the action
        /// returns before it is used.
        /// </remarks>
        public const string BodyParameterName = "body";

        /// <summary>
        /// Whether the data of the command that is not a key is read from the body of the request.
        /// </summary>
        public bool HasBody =>
            DataType == "Command"
            && ResourceType is not (ResourceType.List or ResourceType.Retrieve)
            && NonRouteInvocationParameters.Length > 0;

        /// <summary>
        /// The parameters of the command whose data is read from the body of the request.
        /// </summary>
        public Parameter[] BodyParameters => HasBody ? NonRouteInvocationParameters : [];

        /// <summary>
        /// The name of the generated type the body of the request is read into, with one property per
        /// <see cref="BodyParameters"/>.
        /// </summary>
        public string? BodyClassName =>
            HasBody
                ? $"{GroupPrefix}{MainResource.Pascalize()}{ControllerMethodName.Substring(0, ControllerMethodName.Length - "Async".Length)}Body"
                : null;

        /// <summary>
        /// The name of the controller parameter that reads the query string of the request into
        /// <see cref="QueryStringClassName"/>.
        /// </summary>
        /// <remarks>
        /// The parameter is bound with an empty prefix, so that each value is read by its own name. With the name of the
        /// parameter as its prefix, MVC would read every value under it whenever the query string had a value with that
        /// name, and leave all of them unset.
        /// </remarks>
        public const string QueryStringParameterName = "queryString";

        /// <summary>
        /// Whether the data of the command that is not a key is read from the query string of the request.
        /// </summary>
        public bool HasQueryString =>
            DataType == "Command"
            && ResourceType is ResourceType.List or ResourceType.Retrieve
            && NonRouteInvocationParameters.Length > 0;

        /// <summary>
        /// The parameters of the command whose data is read from the query string of the request.
        /// </summary>
        public Parameter[] QueryStringParameters => HasQueryString ? NonRouteInvocationParameters : [];

        /// <summary>
        /// The name of the generated type the query string of the request is read into, with one property per
        /// <see cref="QueryStringParameters"/>.
        /// </summary>
        /// <summary>
        /// The <c>CommandQueryString</c> attribute that reports what MVC accepts without reporting in the query string of
        /// the command: the required values it does not have and the names it repeats for a value that is not a
        /// collection. <see langword="null"/> when the command reads nothing from the query string.
        /// </summary>
        public string? CommandQueryStringAttributeString =>
            HasQueryString
                ? $"[CommandQueryString({string.Join(", ", QueryStringParameters.Where(parameter => parameter.IsRequired).Select(parameter => $"\"{parameter.Name}\"").Prepend($"typeof({QueryStringClassName})"))})]"
                : null;

        public string? QueryStringClassName =>
            HasQueryString
                ? $"{GroupPrefix}{MainResource.Pascalize()}{ControllerMethodName.Substring(0, ControllerMethodName.Length - "Async".Length)}QueryString"
                : null;

        public Parameter[] RouteParameters =>
            InvocationParameters
                .Where(parameter => KeysByResource.Values.Any(keys => keys.Contains(parameter.Name)))
                .ToArray();

        public Parameter[] ControllerMethodParameters =>
            HasBody
                ? RouteParameters
                    .Append(new Parameter
                    {
                        Name = BodyParameterName,
                        PropertyName = BodyParameterName,
                        Type = BodyClassName!,
                    })
                    .Append(new Parameter {Name = "cancellationToken", PropertyName = "CancellationToken", Type = "System.Threading.CancellationToken"})
                    .ToArray()
                : HasQueryString
                    ? RouteParameters
                        .Append(new Parameter
                        {
                            Name = QueryStringParameterName,
                            PropertyName = QueryStringParameterName,
                            Type = QueryStringClassName!,
                        })
                        .Append(new Parameter {Name = "cancellationToken", PropertyName = "CancellationToken", Type = "System.Threading.CancellationToken"})
                        .ToArray()
                    : InvocationParametersWithToken;

        /// <summary>
        /// The parameters of the controller action that read data of the command from the route or the query string,
        /// by the name of the property of the command they set.
        /// </summary>
        private Parameter[] RequestParameters => HasBody ? RouteParameters : InvocationParameters;

        public string RequestParametersByPropertyString =>
            $"new Dictionary<string, string> {{ {string.Join(", ", RequestParameters.Select(parameter => $"[\"{parameter.PropertyName}\"] = \"{parameter.Name}\""))} }}";

        public string CommandPropertiesString => string.Join(",\n", InvocationParameters
            .Select(parameter => BodyParameters.Contains(parameter)
                ? $"{parameter.PropertyName} = {BodyParameterName}!.{parameter.PropertyName}"
                : QueryStringParameters.Contains(parameter)
                    ? $"{parameter.PropertyName} = {QueryStringParameterName}.{parameter.PropertyName}"
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
                _ when parameter.Name == BodyParameterName && HasBody
                    => $"[FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] {parameter.Type}? {parameter.Name}",
                _ when parameter.Name == QueryStringParameterName && HasQueryString
                    => $"[FromQuery(Name = \"\")] {parameter.Type} {parameter.Name}",
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

        /// <summary>
        /// Whether the property of the command the parameter sets is <c>required</c>.
        /// </summary>
        public bool IsRequired { get; set; }
    }

    private static readonly string ControllerClassTemplate =
        """
        // This file was auto-generated by a source generator
        {{~ if HasResourcesWithBody ~}}
        //
        // The parameter that reads the body of a request (body) is nullable only so that MVC does not add "The body field
        // is required." next to the error of a body that cannot be read (https://github.com/dotnet/aspnetcore/issues/53058).
        // Whenever it is null the model state has an error, an empty body included, so the check of the model state at
        // the start of the action returns before it is used.
        {{~ end ~}}
        {{~ if HasResourcesWithQueryString ~}}
        //
        // The parameter that reads the query string of a request (queryString) is bound with an empty prefix, so that each
        // value is read by its own name. With the name of the parameter as its prefix, MVC would read every value under
        // "queryString." whenever the query string had a value named "queryString", and leave all of them unset.
        {{~ end ~}}
        #nullable enable
        
        using System;
        using System.Collections.Generic;
        using System.ComponentModel.DataAnnotations;
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
        using Microsoft.AspNetCore.Mvc.ModelBinding;
        using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
            {{~ if resource.CommandQueryStringAttributeString ~}}
            {{ resource.CommandQueryStringAttributeString }}
            {{~ end ~}}
            {{~ if resource.ReturnResponseType == 'Task' ~}}
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            {{~ else ~}}
            [ProducesResponseType<{{ resource.ReturnResponseType }}>(StatusCodes.Status200OK)]
            {{~ end ~}}
            public async Task<IActionResult> {{ resource.ControllerMethodName }}({{ resource.ControllerMethodParametersString }})
            {
                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

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
        {{~ for resource in ResourcesDataWithBody ~}}

        /// <summary>
        /// The body of the request of <see cref="{{ ClassName }}.{{ resource.ControllerMethodName }}"/>: the data of
        /// <see cref="{{ resource.FullClassName }}"/> that is not a key. MVC does not validate it: the command validates
        /// its own data when it is invoked.
        /// </summary>
        [ValidateNever]
        public class {{ resource.BodyClassName }}
        {
            {{~ for parameter in resource.BodyParameters ~}}
            public {{ if parameter.IsRequired }}required {{ end }}{{ parameter.Type }} {{ parameter.PropertyName }} { get; init; }
            {{~ end ~}}
        }
        {{~ end ~}}
        {{~ for resource in ResourcesDataWithQueryString ~}}

        /// <summary>
        /// The query string of the request of <see cref="{{ ClassName }}.{{ resource.ControllerMethodName }}"/>: the data
        /// of <see cref="{{ resource.FullClassName }}"/> that is not a key. MVC does not validate it: the command validates
        /// its own data when it is invoked.
        /// </summary>
        [ValidateNever]
        public class {{ resource.QueryStringClassName }}
        {
            {{~ for parameter in resource.QueryStringParameters ~}}
            [FromQuery(Name = "{{ parameter.Name }}"), Display(Name = "{{ parameter.Name }}")]
            public {{ if parameter.IsRequired }}required {{ end }}{{ parameter.Type }} {{ parameter.PropertyName }} { get; init; }
            {{~ end ~}}
        }
        {{~ end ~}}
        """;
}
