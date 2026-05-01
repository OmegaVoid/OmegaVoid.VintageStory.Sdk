using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OmegaVoid.VintageStory.Sdk.Tasks.ModDB;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;
using Microsoft.Build.Framework;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

[UsedImplicitly]
public class Dependencies : BuildTask
{
    #region Params

    [RequiredImplicit] public required ITaskItem[] Dependency { get; set; }
    [RequiredImplicit] public required string OutputDir { get; set; }
    [RequiredImplicit] public required string DependencyDir { get; set; }
    [UsedImplicitly] [Output] public ITaskItem[] ModsDownloaded { get; private set; } = [];

    #endregion

    private async Task<bool> ExecuteAsync()
    {
        try
        {
            var items = new List<ITaskItem>();
            if (Directory.Exists(OutputDir))
                Directory.Delete(OutputDir, true);
            Directory.CreateDirectory(OutputDir);
            if (Directory.Exists(DependencyDir))
                Directory.Delete(DependencyDir, true);
            Directory.CreateDirectory(DependencyDir);
            var dependencies1 = DependencyParser.ParseDependencies(Dependency);
            foreach (var item in dependencies1)
                Log.LogMessage(MessageImportance.High, $"dep {item.Value}");
            dependencies1 = dependencies1.Where(pair => pair.Value.DownloadDep).ToDictionary();
            var dependencies2 = await DependencyParser.FetchModDependencies(dependencies1.Keys);
            foreach (var item in dependencies2)
                Log.LogMessage(MessageImportance.High, $"dep2 {item.Key}");
            var matchedDeps = DependencyParser.MatchDependencies(dependencies1, dependencies2);
            var matchedDeps2 = dependencies1.ToDictionary(pair => pair.Value, pair => dependencies2[pair.Value][pair.Value] );
            foreach (var modRelease in matchedDeps2)
            { 
                Log.LogMessage(MessageImportance.High, $"dep5 {modRelease.Key}");
                var release = modRelease.Value;
                
                await modRelease.DownloadDependency(OutputDir, DependencyDir, helper: Log);
                var path = Path.Combine(OutputDir, release.FileName);
                var path2 = Path.Combine(DependencyDir, release.FileName);
                Log.LogMessage(MessageImportance.High,
                    $"Downloaded {modRelease} to {Path.GetRelativePath(Directory.GetCurrentDirectory(), path2)}");
                Log.LogMessage(MessageImportance.High,
                    $"Extracted {modRelease} to {Path.GetRelativePath(Directory.GetCurrentDirectory(), path.Replace(".zip", ""))}");
                items.Add(new TaskItem(itemSpec: path.Replace(".zip", ""), new Dictionary<string, string>
                {
                    { "ModId", release.IdString }, { "Version", release.Version }, { "Zip", path2 },
                    {
                        "String",
                        modRelease.Key.ToString()
                    },
                    { "Folder", path.Replace(".zip", "") }
                }));
            }

            ModsDownloaded = items.ToArray();
        }
        catch (Exception e)
        {
            var dependencies = string.Join("\n\t",Dependency.Select(item => new Dependency(item).ToString()));
            Log.LogError("Task Dependencies was called with OutputDir = {0}, DependencyDir = {1} on dependencies:\n\t{2}", OutputDir, DependencyDir, dependencies);
            Log.LogErrorFromException(e, true, true, "Dependencies.cs");
        }

        return !Log.HasLoggedErrors;
    }

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();
}