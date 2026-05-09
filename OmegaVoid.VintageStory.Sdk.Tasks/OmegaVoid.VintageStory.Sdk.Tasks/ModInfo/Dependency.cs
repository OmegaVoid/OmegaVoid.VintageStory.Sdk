using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;

public record Dependency(string Id, string Version, bool Fetch = true, bool DownloadDep = true)
    : IComparable<Dependency>
{
    [JsonIgnore] public bool Fetch { get; } = Fetch;
    [JsonIgnore] public bool DownloadDep { get; } = DownloadDep;

    public Dependency(ITaskItem item) : this(item.GetMetadata("Identity"), item.GetMetadata("Version"),
        string.IsNullOrEmpty(item.GetMetadata("Fetch")) || bool.Parse(item.GetMetadata("Fetch")),
        string.IsNullOrEmpty(item.GetMetadata("DownloadDep")) || bool.Parse(item.GetMetadata("DownloadDep")))
    {
    }

    public Dependency(ModDB.ModDBModRelease dbModRelease) : this(dbModRelease.IdString, dbModRelease.Version)
    {
    }

    public override string ToString() => string.IsNullOrEmpty(Version) ? Id : $"{Id}@{Version}";

    public virtual bool Equals(Dependency? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Version == other.Version;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Version);
    }


    // Source - https://stackoverflow.com/a/66270547
// Posted by CTAJIUH, modified by community. See post 'Timeline' for change history
// Retrieved 2026-05-02, License - CC BY-SA 4.0

    public int CompareTo(Dependency? other) => VersionComparer.Compare(this, other);

    private sealed class VersionRelationalComparer : IComparer<Dependency>
    {
        public int Compare(Dependency? x, Dependency? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (y is null) return 1;
            if (x is null) return -1;

            switch (x.Version, y.Version)
            {
                case (_, _) when x.Version.Contains('-') && y.Version.Contains('-'):
                case ("*" or "", "*" or ""):
                    return 0;
                case ("*" or "", _):
                case (_, _) when y.Version.Contains('-'):
                    return -1;
                case (_, "*" or ""):
                case (_, _) when x.Version.Contains('-'):
                    return 1;
            }
            List<int[]> intVersions =
            [
                Array.ConvertAll(x.Version.Split('.'), int.Parse),
                Array.ConvertAll(y.Version.Split('.'), int.Parse)
            ];
            var cmp = intVersions.First().Length.CompareTo(intVersions.Last().Length);
            if (cmp == 0)
                intVersions = intVersions.Select(v =>
                {
                    Array.Resize(ref v, intVersions.Min(x => x.Length));
                    return v;
                }).ToList();
            var strVersions = intVersions.ConvertAll(v => string.Join("", Array.ConvertAll(v,
                i => { return i.ToString($"D{intVersions.Max(x => x.Max().ToString().Length)}"); })));
            var cmpVersions = strVersions.OrderByDescending(i => i).ToList();
            return cmpVersions.First().Equals(cmpVersions.Last()) ? cmp :
                cmpVersions.First().Equals(strVersions.First()) ? 1 : -1;
        }
    }
    
    

    public static IComparer<Dependency> VersionComparer { get; } = new VersionRelationalComparer();
}

public class DependenciesConverter : JsonConverter
{
    public override bool CanConvert(System.Type objectType) =>
        typeof(IEnumerable<Dependency>).IsAssignableFrom(objectType);

    public override object ReadJson(
        JsonReader reader,
        System.Type objectType,
        object? existingValue,
        JsonSerializer serializer) =>
        JObject.Load(reader).Properties()
            .Select((System.Func<JProperty, Dependency>)(prop => new Dependency(prop.Name, ((string)prop.Value)!)))
            .ToList();

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        foreach (var modDependency in (IEnumerable<Dependency>)value!)
        {
            writer.WritePropertyName(modDependency.Id);
            writer.WriteValue(modDependency.Version);
        }

        writer.WriteEndObject();
    }
}