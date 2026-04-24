using Newtonsoft.Json;

namespace OmegaVoid.VintageStory.Sdk.Tasks.Moddb;

[JsonObject]
public struct ModdbModOverview
{
    [JsonProperty("modid")] public int ModId { get; set; }
    [JsonProperty("assetid")] public int AssetId { get; set; }
    [JsonProperty("modidstrs")] public string[] ModIdStrings { get; set; }
    [JsonProperty("author")] public string Author { get; set; }
    [JsonProperty("urlalias")] public string UrlAlias { get; set; }
}