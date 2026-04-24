using Newtonsoft.Json;

namespace OmegaVoid.VintageStory.Sdk.Tasks.Moddb;

[JsonObject]
public struct ModdbModIndex
{
    [JsonProperty(PropertyName = "statuscode")]
    public string StatusCode { get; set; }

    [JsonProperty(PropertyName = "mods")] public ModdbModOverview[] Mods { get; set; }
}