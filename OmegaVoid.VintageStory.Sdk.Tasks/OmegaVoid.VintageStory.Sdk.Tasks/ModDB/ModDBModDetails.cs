using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;

namespace OmegaVoid.VintageStory.Sdk.Tasks.ModDB;

[JsonObject(NamingStrategyType = typeof(LowerCaseNamingStrategy))]
public struct ModDBModDetails
{
    [JsonProperty] public int ModId { get; set; }
    [JsonProperty] public int AssetId { get; set; }
    [JsonProperty] public string Author { get; set; }
    [JsonProperty] public string UrlAlias { get; set; }
    [JsonProperty] public ModDBModRelease[] Releases { get; set; }

    [JsonIgnore] public ReadOnlyDictionary<Dependency, ModDBModRelease> ModReleases { get; private set; }
    [JsonIgnore] public ReadOnlyDictionary<string, ModDBModRelease> ModReleaseVersions { get; private set; }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
        ModReleases = Releases.ToDictionary(release => new Dependency(release)).AsReadOnly();
        ModReleaseVersions = Releases.ToDictionary(release => release.Version, release => release).AsReadOnly();
    }

    public ModDBModRelease this[Dependency dependency] => ModReleases[dependency];
    public ModDBModRelease this[string version] => ModReleaseVersions[version];
}