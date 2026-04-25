using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;


namespace OmegaVoid.VintageStory.Sdk.Tasks.Moddb;

[JsonObject]
public struct ModdbModRelease : IEquatable<ModdbModRelease>
{
    [JsonProperty("releaseid")] public int ReleaseId { get; set; }
    [JsonProperty("fileid")] public int FileId { get; set; }
    [JsonProperty("mainfile")] public string MainFile { get; set; }
    [JsonProperty("filename")] public string FileName { get; set; }
    [JsonProperty("modidstr")] public string IdString { get; set; }
    [JsonProperty("modversion")] public string Version { get; set; }

    public async Task DownloadDependency(string outputDir, CancellationToken cancellationToken = default)
    {
        
        var filePath = Path.Combine(outputDir, FileName.Replace(".zip", ""));
        await using var downloadStream = await DependencyParser.Client.GetStreamAsync(MainFile, cancellationToken);
        await using var fileStream = new FileStream(Path.Combine(outputDir, FileName), FileMode.Create,
            FileAccess.Write|FileAccess.Read);
        await downloadStream.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
        await ZipFile.ExtractToDirectoryAsync(fileStream, filePath, cancellationToken);
    }

    public static explicit operator Dependency(ModdbModRelease release) => new Dependency(release);
    public static explicit operator ModdbModRelease(KeyValuePair<Dependency, ModdbModDetails> pair) => pair.GetRelease();

    public bool Equals(ModdbModRelease other) => ReleaseId == other.ReleaseId && FileId == other.FileId &&
                                                 MainFile == other.MainFile && FileName == other.FileName &&
                                                 IdString == other.IdString && Version == other.Version;

    public override bool Equals(object? obj) => obj is ModdbModRelease other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(ReleaseId, FileId, MainFile, FileName, IdString, Version);
    
    public override string ToString() => ((Dependency)this).ToString();
}