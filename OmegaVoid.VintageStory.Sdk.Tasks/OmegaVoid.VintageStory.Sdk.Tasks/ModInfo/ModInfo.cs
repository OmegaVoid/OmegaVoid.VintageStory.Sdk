using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public record ModInfo
{
    [JsonProperty] public List<string> Authors { get; set; } = [];


    [JsonRequired]
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public ModType ModType { get; set; } = ModType.Code;

    [JsonRequired] public string Name { get; set; } = "";
    [JsonProperty] public string Version { get; set; } = "";
    [JsonProperty] public string? Description { get; set; }

    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public AppSide Side { get; set; } = AppSide.Universal;

    [JsonProperty] public bool RequiredOnClient { get; set; } = true;
    [JsonProperty] public bool RequiredOnServer { get; set; } = true;
    [JsonProperty] public string Website { get; set; } = "";
    [JsonProperty] public string? IconPath { get; set; }

    [JsonProperty] public List<string> Contributors { get; set; } = [];

    [JsonProperty] public int TextureSize { get; set; }
    [JsonProperty("modid")] public string ModID { get; set; } = null!;
    [JsonProperty] public string? NetworkVersion { get; set; }

    [JsonProperty]
    [JsonConverter(typeof(DependenciesConverter))]
    public List<Dependency> Dependencies { get; set; } = [];
}