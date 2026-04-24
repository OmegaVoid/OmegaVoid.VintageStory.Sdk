using Newtonsoft.Json;

namespace OmegaVoid.VintageStory.Sdk.Tasks.Moddb;

[JsonObject]
public struct ModdbModDetails
{
    [JsonProperty("modid")] public int ModId { get; set; }
    [JsonProperty("assetid")] public int AssetId { get; set; }
    [JsonProperty("author")] public string Author { get; set; }
    [JsonProperty("urlalias")] public string UrlAlias { get; set; }
    [JsonProperty("releases")] public ModdbModRelease[] Releases { get; set; }

    
}

[JsonObject]
public struct ModdbModDetailsPage
{
    [JsonProperty(PropertyName = "statuscode")]
    public string StatusCode { get; set; }

    [JsonProperty(PropertyName = "mod")] public ModdbModDetails Mods { get; set; }
}