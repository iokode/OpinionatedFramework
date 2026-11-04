using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllers.Config;

/// <summary>
/// Serves the controllers of the "Public" resource group under <c>/public</c>, the way an application decides where
/// each group is served.
/// </summary>
public class PublicGroupRouteConvention : IControllerModelConvention
{
    private static readonly AttributeRouteModel prefix = new() { Template = "public" };

    public void Apply(ControllerModel controller)
    {
        if (!controller.ControllerName.StartsWith("Public"))
        {
            return;
        }

        foreach (var selector in controller.Selectors)
        {
            selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(prefix, selector.AttributeRouteModel);
        }
    }
}
