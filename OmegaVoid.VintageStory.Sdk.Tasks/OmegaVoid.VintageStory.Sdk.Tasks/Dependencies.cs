using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OmegaVoid.VintageStory.Sdk.Tasks.ModDB;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using Task = System.Threading.Tasks.Task;

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
            var items = new List<DependencyOutputTaskItem>();
            if (Directory.Exists(OutputDir))
                Directory.Delete(OutputDir, true);
            Directory.CreateDirectory(OutputDir);
            if (Directory.Exists(DependencyDir))
                Directory.Delete(DependencyDir, true);
            Directory.CreateDirectory(DependencyDir);

            var allDeps = new List<Dependency>();

            ICollection<Dependency> ProcessHandledOutput(KeyValuePair<TaskItem, ICollection<Dependency>> depData)
            {
                items.Add((DependencyOutputTaskItem)depData.Key);
                allDeps.AddRange(depData.Value);
                return depData.Value;
            }


            var nestedDepsToResolve =
                (await HandleDependencies(DependencyParser.ParseDependencies(Dependency))).SelectMany(
                    ProcessHandledOutput);

            while (nestedDepsToResolve.Any())
            {
                Log.LogMessage(MessageImportance.High, "Found dependencies:\n{0}",
                    nestedDepsToResolve.Aggregate("", (current, dependency) => current + $"\t{dependency}\n"));
                var nextNested = (await HandleDependencies(nestedDepsToResolve)).SelectMany(ProcessHandledOutput);
                nestedDepsToResolve = nextNested;
            }

            Log.LogMessage(MessageImportance.High, "All dependencies found:\n{0}",
                allDeps.Aggregate("", (current, dependency) => current + $"\t{dependency}\n"));

            var test = allDeps.GroupBy(dep => dep.Id).Select(grouping => grouping.OrderDescending().First());
            var removed = allDeps.Except(test);
            
            Log.LogMessage(MessageImportance.High, "Duplicate dependencies removed:\n{0}",
                removed.Aggregate("", (current, dependency) => current + $"\t{dependency}\n"));
            var itemsDict = items.ToDictionary(item => item.Dependency);
            var removedItems = itemsDict.IntersectBy(removed, pair => pair.Key);
            var keepItems = itemsDict.ExceptBy(removed, pair => pair.Key);

            foreach (var removedItem in removedItems)
            {
                Directory.Delete(removedItem.Value.FolderPath, true);
                File.Delete(removedItem.Value.ZipPath);
            }
            
            Log.LogMessage(MessageImportance.High, "Final dependencies:\n{0}",
                keepItems.Aggregate("", (current, dependency) => current + $"\t{dependency.Key}\n"));
            ModsDownloaded = keepItems.Select(pair => (TaskItem)pair.Value).ToArray<ITaskItem>();
        }
        catch (Exception e)
        {
            var dependencies = string.Join("\n\t", Dependency.Select(item => new Dependency(item).ToString()));
            Log.LogError(
                "Task Dependencies was called with OutputDir = {0}, DependencyDir = {1} on dependencies:\n\t{2}",
                OutputDir, DependencyDir, dependencies);
            Log.LogErrorFromException(e, true, true, "Dependencies.cs");
        }

        return !Log.HasLoggedErrors;
    }

    private async Task<KeyValuePair<TaskItem, ICollection<Dependency>>[]> HandleDependencies(
        IEnumerable<Dependency> nestedDeps) =>
        await Task.WhenAll(
            (await CollectDependencies(nestedDeps)).Select(async pair => await ResolveDependency(pair)));

    private async Task<KeyValuePair<TaskItem, ICollection<Dependency>>[]> HandleDependencies(
        Dictionary<string, Dependency> nestedDepsDict) =>
        (await Task.WhenAll(
            (await CollectDependencies(nestedDepsDict)).Select(async pair => await ResolveDependency(pair))));

    private async Task<KeyValuePair<TaskItem, ICollection<Dependency>>> ResolveDependency(
        KeyValuePair<Dependency, ModDBModRelease> modData)
    {
        Log.LogMessage(MessageImportance.Low, $"dep5 {modData.Key}");
        var release = modData.Value;
        var nestedDeps = new List<Dependency>();

        var nestedDeps1 =
            (await modData.DownloadDependency(OutputDir, DependencyDir, helper: Log))?.SelectMany(info =>
                info.Dependencies);
        if (nestedDeps1 != null) nestedDeps.AddRange(nestedDeps1);
        var path = Path.Combine(OutputDir, release.FileName).Replace(".zip", "");
        var path2 = Path.Combine(DependencyDir, release.FileName);
        Log.LogMessage(MessageImportance.High,
            $"Downloaded {modData} to {Path.GetRelativePath(Directory.GetCurrentDirectory(), path2)}");
        Log.LogMessage(MessageImportance.High,
            $"Extracted {modData} to {Path.GetRelativePath(Directory.GetCurrentDirectory(), path)}");
        return new KeyValuePair<TaskItem, ICollection<Dependency>>(modData.ToTaskItem(path2, path), nestedDeps);
    }

    private async Task<Dictionary<Dependency, ModDBModRelease>> CollectDependencies(
        IEnumerable<Dependency> dependencies) =>
        await CollectDependencies(dependencies.Where(dependency => dependency.Id != "game").ToDictionary(dep => dep.Id, dep => dep));

    private async Task<Dictionary<Dependency, ModDBModRelease>> CollectDependencies(
        Dictionary<string, Dependency> dependencies1)
    {
        foreach (var item in dependencies1)
            Log.LogMessage(MessageImportance.Low, $"dep {item.Value}");
        dependencies1 = dependencies1.Where(pair => pair.Value.DownloadDep).ToDictionary();
        var dependencies2 = await DependencyParser.FetchModDependencies(dependencies1.Keys);
        foreach (var item in dependencies2)
            Log.LogMessage(MessageImportance.Low, $"dep2 {item.Key}");
        var matchedDeps2 =
            dependencies1.ToDictionary(pair => pair.Value, pair => dependencies2[pair.Value][pair.Value]);
        return matchedDeps2;
    }

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();
}