using Newtonsoft.Json;

namespace OmegaVoid.VintageStory.Sdk.Tasks.ModDB;

[JsonObject(NamingStrategyType = typeof(LowerCaseNamingStrategy), MemberSerialization = MemberSerialization.OptOut)]
public struct ModDBModIndex
{
    public string StatusCode { get; set; }

    public ModDBModOverview[] Mods { get; set; }
}