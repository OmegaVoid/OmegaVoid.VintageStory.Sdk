using Newtonsoft.Json;

namespace OmegaVoid.VintageStory.Sdk.Tasks.Moddb;

[JsonObject]
public struct ModdbModDetailsPage
{
    [JsonProperty(PropertyName = "statuscode")]
    public string StatusCode { get; set; }

    [JsonProperty(PropertyName = "mod")] public ModdbModDetails Mods { get; set; }
}