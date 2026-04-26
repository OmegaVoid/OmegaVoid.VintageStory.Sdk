using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;

public record Dependency(string Id, string Version, bool Fetch = true)
{
#if NETSTANDARD2_0_OR_GREATER
    public string Id { get; private set; } = Id;
    public string Version { get; private set; } = Version;
#else
    public string Id { get; init; } = Id;
    public string Version { get; init; } = Version;
#endif

    [JsonIgnore]
    public bool Fetch { get; set; } = Fetch;

    public Dependency(ITaskItem item) : this(item.GetMetadata("Identity"), item.GetMetadata("Version"), string.IsNullOrEmpty(item.GetMetadata("Fetch")) || bool.Parse(item.GetMetadata("Fetch")))
    {
    }
    public Dependency(Moddb.ModdbModRelease release) : this(release.IdString, release.Version)
    {
    }

    public override string ToString() => string.IsNullOrEmpty(Version) ? Id : $"{Id}@{Version}";
}

public class DependenciesConverter : JsonConverter
{
    public override bool CanConvert(System.Type objectType)
    {
        return typeof (IEnumerable<Dependency>).IsAssignableFrom(objectType);
    }

    public override object ReadJson(
        JsonReader reader,
        System.Type objectType,
        object? existingValue,
        JsonSerializer serializer)
    {
        return JObject.Load(reader).Properties().Select((System.Func<JProperty, Dependency>) (prop => new Dependency(prop.Name, ((string) prop.Value)!))).ToList().AsReadOnly();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        foreach (var modDependency in (IEnumerable<Dependency>) value!)
        {
            writer.WritePropertyName(modDependency.Id);
            writer.WriteValue(modDependency.Version);
        }
        writer.WriteEndObject();
    }
}