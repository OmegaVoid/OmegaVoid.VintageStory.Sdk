using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;


namespace OmegaVoid.VintageStory.Sdk.Tasks.ModDB;

[JsonObject(NamingStrategyType = typeof(LowerCaseNamingStrategy))]
public struct ModDBModRelease : IEquatable<ModDBModRelease>
{
    [JsonProperty] public int ReleaseId { get; set; }
    [JsonProperty] public int FileId { get; set; }
    [JsonProperty] public string MainFile { get; set; }
    [JsonProperty] public string FileName { get; set; }
    [JsonProperty("modidstr")] public string IdString { get; set; }
    [JsonProperty("modversion")] public string Version { get; set; }

    public async Task DownloadDependency(string outputDir, string? dependencyDir = null, bool fetch = true,
        CancellationToken cancellationToken = default)
    {
        dependencyDir ??= Path.Combine(outputDir, "Mods");
        var filePath = Path.Combine(outputDir, FileName.Replace(".zip", ""));
        await using var downloadStream = await DependencyParser.Client.GetStreamAsync(MainFile, cancellationToken);
        await using var fileStream = new FileStream(Path.Combine(dependencyDir, FileName), FileMode.Create,
            FileAccess.Write | FileAccess.Read);
        await downloadStream.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
        if (fetch)
            await ZipFile.ExtractToDirectoryAsync(fileStream, filePath, cancellationToken);
    }

    public static explicit operator Dependency(ModDBModRelease dbModRelease) => new(dbModRelease);

    public static explicit operator ModDBModRelease(KeyValuePair<Dependency, ModDBModDetails> pair) =>
        pair.GetRelease();

    public bool Equals(ModDBModRelease other) => ReleaseId == other.ReleaseId && FileId == other.FileId &&
                                                 MainFile == other.MainFile && FileName == other.FileName &&
                                                 IdString == other.IdString && Version == other.Version;

    public override bool Equals(object? obj) => obj is ModDBModRelease other && Equals(other);

    public static bool operator ==(ModDBModRelease left, ModDBModRelease right) => left.Equals(right);

    public static bool operator !=(ModDBModRelease left, ModDBModRelease right) => !left.Equals(right);

    public override int GetHashCode() => HashCode.Combine(ReleaseId, FileId, MainFile, FileName, IdString, Version);

    public override string ToString() => ((Dependency)this).ToString();
}