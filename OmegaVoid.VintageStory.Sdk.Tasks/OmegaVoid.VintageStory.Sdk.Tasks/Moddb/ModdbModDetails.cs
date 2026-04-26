using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;

namespace OmegaVoid.VintageStory.Sdk.Tasks.Moddb;

[JsonObject]
public struct ModdbModDetails
{
    [JsonProperty("modid")] public int ModId { get; set; }
    [JsonProperty("assetid")] public int AssetId { get; set; }
    [JsonProperty("author")] public string Author { get; set; }
    [JsonProperty("urlalias")] public string UrlAlias { get; set; }
    [JsonProperty("releases")] public ModdbModRelease[] Releases { get; set; }

    [JsonIgnore] public ReadOnlyDictionary<Dependency, ModdbModRelease> ModReleases { get; private set; }
    [JsonIgnore] public ReadOnlyDictionary<string, ModdbModRelease> ModReleaseVersions { get; private set; }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
#if NETSTANDARD2_0_OR_GREATER
        ModReleases = new ReadOnlyDictionary<Dependency, ModdbModRelease>(Releases.ToDictionary(release => new Dependency(release)));
        ModReleaseVersions = new ReadOnlyDictionary<string, ModdbModRelease>(Releases.ToDictionary(release => release.Version, release => release));
#else
        ModReleases = Releases.ToDictionary(release => new Dependency(release)).AsReadOnly();
        ModReleaseVersions = Releases.ToDictionary(release => release.Version, release => release).AsReadOnly();
#endif
    }

    public ModdbModRelease this[Dependency dependency] => ModReleases[dependency];
    public ModdbModRelease this[string version] => ModReleaseVersions[version];
}