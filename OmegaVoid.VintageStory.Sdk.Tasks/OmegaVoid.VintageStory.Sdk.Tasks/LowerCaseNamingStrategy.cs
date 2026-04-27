using Microsoft.Build.Framework;
using Newtonsoft.Json.Serialization;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

public class LowerCaseNamingStrategy : NamingStrategy
{
    /// <summary>
    /// Resolves the specified property name.
    /// </summary>
    /// <param name="name">The property name to resolve.</param>
    /// <returns>The resolved property name.</returns>
    protected override string ResolvePropertyName(string name)
    {
        return name.ToLower();
    }
}
[MeansImplicitUse]
public class RequiredImplicitAttribute : RequiredAttribute;