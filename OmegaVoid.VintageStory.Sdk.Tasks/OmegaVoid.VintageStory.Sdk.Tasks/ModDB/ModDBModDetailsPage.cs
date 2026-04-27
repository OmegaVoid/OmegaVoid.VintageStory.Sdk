using Newtonsoft.Json;

namespace OmegaVoid.VintageStory.Sdk.Tasks.ModDB;

[JsonObject(NamingStrategyType = typeof(LowerCaseNamingStrategy), MemberSerialization = MemberSerialization.OptOut)]
public struct ModDBModDetailsPage
{
    public string StatusCode { get; set; }

    public ModDBModDetails Mod { get; set; }
}