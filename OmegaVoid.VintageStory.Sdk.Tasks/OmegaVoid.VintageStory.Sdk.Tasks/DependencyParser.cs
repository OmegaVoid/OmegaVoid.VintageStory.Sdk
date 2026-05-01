using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Build.Framework;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.Swift;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using OmegaVoid.VintageStory.Sdk.Tasks.ModDB;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;
using Task = System.Threading.Tasks.Task;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

public static class DependencyParser
{
    public static readonly HttpClient Client = new() { BaseAddress = new Uri("https://mods.vintagestory.at") };

    public static Dictionary<string, Dependency> ParseDependencies(ITaskItem[] dependencies)
    {
        var deps = from idep in dependencies
            let dep = new Dependency(idep)
            where dep.Id != "game"
            select new KeyValuePair<string, Dependency>(dep.Id, dep);
        return deps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static async Task<Dictionary<Dependency, ModDBModDetails>> FetchModDependencies(
        IEnumerable<string> dependencies)
    {
        var dependencyList = new Dictionary<Dependency, ModDBModDetails>();
        foreach (var mod in dependencies)
        {
            using var resp = await Client.GetAsync($"api/mod/{mod}");
            var details = JsonConvert.DeserializeObject<ModDBModDetailsPage>(await resp.Content.ReadAsStringAsync())
                .Mod;
            foreach (var release in details.Releases)
                dependencyList.Add((Dependency)release, details);
        }

        return dependencyList;
    }

    public static Dictionary<Dependency, ModDBModDetails> MatchDependencies(
        Dictionary<string, Dependency> dependencies1, Dictionary<Dependency, ModDBModDetails> dependencies2)
    {
        var foo = dependencies2.Where(dependency => dependencies1.ContainsValue(dependency.Key));
        return foo.ToDictionary(x => x.Key, x => x.Value);
    }

    // public static async Task DownloadDependency(KeyValuePair<Dependency, ModDBModDetails> dependency, string outputDir,
    //     string? dependencyDir = null, CancellationToken cancellationToken = default) =>
    //     await ((ModDBModRelease)dependency).DownloadDependency(outputDir, dependencyDir, cancellationToken);


    extension(KeyValuePair<Dependency, ModDBModDetails> pair)
    {
        public ModDBModRelease GetRelease() => pair.Value[pair.Key];
        
        public async Task DownloadDependency(string outputDir, string? dependencyDir = null, TaskLoggingHelper? helper = null, 
            CancellationToken cancellationToken = default) =>
            await ((ModDBModRelease)pair).DownloadDependency(outputDir, dependencyDir, pair.Key.Fetch, pair.Key.DownloadDep, helper, cancellationToken);
    }
    
    extension(KeyValuePair<Dependency, ModDBModRelease> pair)
    {
        public async Task DownloadDependency(string outputDir, string? dependencyDir = null, TaskLoggingHelper? helper = null, 
            CancellationToken cancellationToken = default) =>
            await (pair.Value).DownloadDependency(outputDir, dependencyDir, pair.Key.Fetch, pair.Key.DownloadDep, helper, cancellationToken);
    }
}