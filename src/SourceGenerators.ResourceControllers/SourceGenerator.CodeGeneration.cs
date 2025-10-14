using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public string BaseRoute => ResourcesData[0].Resource.Pluralize().Kebaberize();
        public string ClassName => $"{ResourcesData[0].Resource.Pascalize()}ResourceController";
        public string Namespace => $"{AssemblyNamespace}.Resources.Controllers";
        public string ClassFileName => $"{ClassName}.g.cs";
    }

    public abstract class ResourceData
    {
        public abstract string DataType { get; } 
        public required string Resource { get; set; }
        public required string ClassName { get; set; }
        public required string Namespace { get; set; }
        public required ResourceType ResourceType { get; set; }
        public string? KeyName { get; set; }
        public string? Action { get; set; }
        public Parameter[] InvocationParameters { get; set; } = Array.Empty<Parameter>();
        public Parameter[] InvocationParametersWithoutCancellationToken => InvocationParameters.Where(param => param.Name != "cancellationToken").ToArray();
        public virtual Parameter[] ControllerMethodParameters => InvocationParameters.Any(parameter => parameter.Type == "System.Threading.CancellationToken")
            ? InvocationParameters
            : InvocationParameters.Concat(new[] {new Parameter {Name = "cancellationToken", Type = "System.Threading.CancellationToken"}}).ToArray();
        public string? ConstructorKeyName => InvocationParametersWithoutCancellationToken.Length > 0 && ResourceType != ResourceType.List ? InvocationParametersWithoutCancellationToken[0].Name : null;
        public string FullClassName => $"{Namespace}.{ClassName}";
        public bool ThereIsResourceId => InvocationParametersWithoutCancellationToken.Any();

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

                if(ResourceType == ResourceType.Retrieve && !ThereIsResourceId)
                {
                    builder.Append($"/{Resource.Kebaberize()}/");
                }

                if (KeyName != null)
                {
                    builder.Append($"{KeyName.Kebaberize()}/");
                }

                if (ConstructorKeyName != null)
                {
                    builder.Append($"{{{ConstructorKeyName}}}/");
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

                if (KeyName != null)
                {
                    builder.Append(KeyName.Pascalize());
                }

                if (Action != null)
                {
                    builder.Append(Action.Pascalize());
                }

                builder.Append("Async");
                return builder.ToString();
            }
        }
    }

    public class QueryData : ResourceData
    {
        public override string DataType => "Query";
    }

    public class CommandData : ResourceData
    {
        public string? GenericArgument { get; set; }
        public override string DataType => "Command";
    }

    public class Parameter
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
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
        using IOKode.OpinionatedFramework.Commands;
        using IOKode.OpinionatedFramework.Facades;
        using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Mvc;

        namespace {{ Namespace }};

        [ApiController]
        [Route("{{ BaseRoute }}")]
        public partial class {{ ClassName }} : Controller
        {
            {{~ for resource in ResourcesData ~}}
            {{ resource.HttpAttribute }}
            [SourceCommand("{{ resource.FullClassName }}")]
            public async Task<IActionResult> {{ resource.ControllerMethodName }}({{ for parameter in resource.ControllerMethodParameters }}{{ if for.first && resource.ResourceType == 'List' }}[FromQuery] {{ end }}{{ parameter.Type }} {{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }})
            {
                try
                {
                    {{~ if resource.DataType == 'Query' ~}}
                    var result = await {{ resource.FullClassName }}.InvokeAsync({{ for parameter in resource.InvocationParameters }}{{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }});
                    return Ok(result);
                    {{~ else if resource.DataType == 'Command' ~}}
                    var commandExecutor = Locator.Resolve<ICommandExecutor>();
                    var command = new {{ resource.FullClassName }}({{ for parameter in resource.InvocationParameters }}{{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }});
                    {{~ if resource.GenericArgument ~}}
                    var result = await commandExecutor.InvokeAsync<{{ resource.FullClassName }}, {{ resource.GenericArgument }}>(command, cancellationToken);
                    return Ok(result);
                    {{~ else ~}}
                    await commandExecutor.InvokeAsync<{{ resource.FullClassName }}>(command, cancellationToken);
                    return NoContent();
                    {{~ end ~}}
                    {{~ end ~}}
                }
                catch (EntityNotFoundException)
                {
                    return NotFound();
                }
                catch (EmptyResultException)
                {
                    return NotFound();
                }
                catch (NonUniqueResultException)
                {
                    return Problem
                    (
                        title: "Internal Server Error",
                        detail: "Multiple results found when expecting a unique result.",
                        statusCode: StatusCodes.Status500InternalServerError
                    );
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
        """;
}