using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Build.Framework;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmegaVoid.VintageStory.Sdk.Tasks.Moddb;
using OmegaVoid.VintageStory.Sdk.Tasks.ModInfo;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

public static class DependencyParser
{
    public static readonly HttpClient Client = new HttpClient { BaseAddress = new Uri("https://mods.vintagestory.at") };

    public static Dictionary<string, Dependency> ParseDependencies(ITaskItem[] dependencies)
    {
        var deps = from idep in dependencies
            let dep = new Dependency(idep)
            where dep.Id != "game"
            select new KeyValuePair<string, Dependency>(dep.Id, dep);
        return deps.ToDictionary();
    }

    public static async Task<Dictionary<Dependency, ModdbModDetails>> FetchModDependencies(
        IEnumerable<string> dependencies)
    {
        var dependencyList = new Dictionary<Dependency, ModdbModDetails>();
        foreach (var mod in dependencies)
        {
            using var resp = await Client.GetAsync($"api/mod/{mod}");
            var details = JsonConvert.DeserializeObject<ModdbModDetailsPage>(await resp.Content.ReadAsStringAsync())
                .Mods;
            foreach (var release in details.Releases)
                dependencyList.Add((Dependency)release, details);
        }

        return dependencyList;
    }

    public static Dictionary<Dependency, ModdbModDetails> MatchDependencies(
        Dictionary<string, Dependency> dependencies1, Dictionary<Dependency, ModdbModDetails> dependencies2)
    {
        var foo = from dependency in dependencies2 where dependencies1.ContainsValue(dependency.Key) select dependency;
        return foo.ToDictionary();
    }

    public static async Task DownloadDependency(KeyValuePair<Dependency, ModdbModDetails> dependency, string outputDir, CancellationToken cancellationToken = default)
    {
        await ((ModdbModRelease)dependency).DownloadDependency(outputDir, cancellationToken);
    }


    extension(KeyValuePair<Dependency, ModdbModDetails> pair)
    {
        public ModdbModRelease GetRelease() => pair.Value[pair.Key];
    }

}