using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;
using Task = System.Threading.Tasks.Task;


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

    public async Task<List<ModInfo.ModInfo>?> DownloadDependency(string outputDir, string? dependencyDir = null, bool fetch = true,
        bool downloadDep = true, TaskLoggingHelper? log = null,
        CancellationToken cancellationToken = default)
    {
        if (downloadDep == false)
        {
            log?.LogMessage(MessageImportance.High, "skipping file {0}", FileName);
            return null;
        }

        log?.LogMessage(MessageImportance.High, "DownloadDependency file {0}", FileName);

        dependencyDir ??= Path.Combine(outputDir, "Mods");
        var filePath = Path.Combine(outputDir, FileName.Replace(".zip", ""));
        await using var downloadStream = await DependencyParser.Client.GetStreamAsync(MainFile, cancellationToken);
        await using var fileStream = new FileStream(Path.Combine(dependencyDir, FileName), FileMode.Create,
            FileAccess.Write | FileAccess.Read);
        await downloadStream.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
        log?.LogMessage(MessageImportance.High, "downloading file {0}", FileName);
        DirectoryInfo dir;
        if (fetch)
        {
            log?.LogMessage(MessageImportance.High, "extracting file {0} to {1}", FileName, filePath);
            await ZipFile.ExtractToDirectoryAsync(fileStream, filePath, cancellationToken);
            dir = new DirectoryInfo(filePath);
        }
        else
        {
            log?.LogMessage(MessageImportance.High, "downloaded file {0}", FileName);
            var tempSubdirectory = Directory.CreateTempSubdirectory();
            log?.LogMessage(MessageImportance.High, "extracting file {0} to temporary directory {1}", FileName, tempSubdirectory);
            await ZipFile.ExtractToDirectoryAsync(fileStream, tempSubdirectory.FullName, cancellationToken);
            dir = tempSubdirectory;
        }

        var modInfos = new List<ModInfo.ModInfo>();
        foreach (var fileInfo in dir.EnumerateFiles("modinfo.json"))
        {
            var modinfoText = await File.ReadAllTextAsync(fileInfo.FullName, cancellationToken);
            var modinfo = JsonConvert.DeserializeObject<ModInfo.ModInfo>(modinfoText);
            if(modinfo is not null)
                modInfos.Add(modinfo);
        }

        return modInfos;
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