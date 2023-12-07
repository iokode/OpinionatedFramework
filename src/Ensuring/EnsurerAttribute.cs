using System;

namespace IOKode.OpinionatedFramework.Ensuring;

/// <summary>
/// Attribute used to decorate static classes that are responsible for performing validation checks in the Ensure API.
/// When a class is decorated with the EnsurerAttribute, it signifies that the class is a part of the Ensure
/// validation mechanism, it is expected to be static, and its methods are expected to return boolean values 
/// based on the validation results.
/// </summary>
/// <remarks>
/// This attribute can only be applied to classes due to the AttributeTargets.Class parameter in AttributeUsage.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class EnsurerAttribute : Attribute
{
}