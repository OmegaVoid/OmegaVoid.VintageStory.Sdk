using Newtonsoft.Json;

namespace OmegaVoid.VintageStory.Sdk.Tasks.ModDB;

[JsonObject(NamingStrategyType = typeof(LowerCaseNamingStrategy), MemberSerialization = MemberSerialization.OptOut)]
public struct ModDBModOverview
{
    public int ModId { get; set; }
    public int AssetId { get; set; }
    [JsonProperty("modidstrs")] public string[] ModIdStrings { get; set; }
    public string Author { get; set; }
    public string UrlAlias { get; set; }
}