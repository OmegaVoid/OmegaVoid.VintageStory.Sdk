using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OmegaVoid.VintageStory.Sdk.Tasks.Moddb;

namespace OmegaVoid.VintageStory.Sdk.Tasks;

using System;
using Microsoft.Build.Framework;
using Newtonsoft.Json;

public record Dependency
{
    public string Id { get; }
    public string Version { get; }

    public Dependency(ITaskItem item)
    {
        Id = item.GetMetadata("Identity");
        Version = item.GetMetadata("Version");
    }

    public override string ToString() => $"{Id} {Version}";
}

public class Dependencies : Microsoft.Build.Utilities.Task
{
    public ITaskItem[] Dependency { get; set; }
    public string OutputDir { get; set; }

    public async Task<bool> ExecuteAsync()
    {
        Log.LogWarning(OutputDir);
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://mods.vintagestory.at");
        using var response = await client.GetAsync("api/mods");
        var content = await response.Content.ReadAsStringAsync();
        var index = JsonConvert.DeserializeObject<ModdbModIndex>(content).Mods;
        var deps = (from idep in Dependency let dep = new Dependency(idep) where dep.Id != "game" select dep);
        var modd = (from mod in index
            where mod.ModIdStrings.Intersect(deps.Select(dependency => dependency.Id)).Any()
            select mod).ToArray();
        var dependencyList = new Dictionary<Dependency, ModdbModDetails>();
        foreach (var mod in modd)
        {
            using var resp = await client.GetAsync($"api/mod/{mod.ModId}");
            var details = JsonConvert.DeserializeObject<ModdbModDetailsPage>(await resp.Content.ReadAsStringAsync())
                .Mods;
            var dep = deps.Where(dependency =>
                details.Releases.Select(release => release.IdString).Any(s => s == dependency.Id)).ToArray();
            if (dep.Length > 0)
                dependencyList.Add(dep[0], details);
            // Log.LogWarning(JsonConvert.SerializeObject(details));
        }

        foreach (var depen in dependencyList)
        {
            ;
            using (var webClient = new HttpClient())
            {
                var release = depen.Value.Releases.Where(release => release.Version == depen.Key.Version).ToArray()
                    .First();
                await using var downloadStream = await client.GetStreamAsync(release.MainFile);
                await using var fileStream = new FileStream($"{OutputDir}/{release.FileName}", FileMode.Create,
                    FileAccess.Write);
                await ZipFile.ExtractToDirectoryAsync(downloadStream, $"{OutputDir}/{release.FileName.Replace(".zip","")}");
            }

            Log.LogWarning(depen.Key.ToString());
            Log.LogWarning(JsonConvert.SerializeObject(depen.Value));
        }
        // Log.LogWarning(string.Join("\n", deps));
        // Log.LogWarning(string.Join("\n", JsonConvert.SerializeObject(modd)));


        return true;
    }

    public override bool Execute() => Task.Run(ExecuteAsync).GetAwaiter().GetResult();
}